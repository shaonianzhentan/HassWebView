using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.Accessibility;
using System.Text.RegularExpressions;
using Action = Android.Views.Accessibility.Action;

namespace HassWebView.AndroidService.AdSkipping;

[Service(Label = "HassWebView Ad Skipping Service",
         Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
         Exported = false)]
[IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
public class AdSkippingService : AccessibilityService
{
    public static GkdRuleManager RuleManager { get; } = new();

    public override void OnAccessibilityEvent(AccessibilityEvent? e)
    {
        if (e?.Source == null || string.IsNullOrEmpty(e.PackageName)) return;

        var appRules = RuleManager.GetRulesForApp(e.PackageName);
        if (appRules == null || appRules.Groups.Count == 0)
        {
            RecycleNode(e.Source);
            return;
        }

        var rootNode = FindRootNode(e.Source); // e.Source is recycled inside
        if (rootNode == null) return;

        foreach (var group in appRules.Groups)
        {
            foreach (var rule in group.Rules)
            {
                var nodes = FindNodesBySelector(rootNode, rule.Matches);
                if (nodes.Count > 0)
                {
                    var nodeToClick = nodes[0];
                    nodeToClick.PerformAction(Action.Click);
                    RecycleNode(nodeToClick);
                    foreach (var node in nodes.Skip(1)) RecycleNode(node);
                    RecycleNode(rootNode);
                    return;
                }
            }
        }
        RecycleNode(rootNode);
    }

    private static void RecycleNode(AccessibilityNodeInfo? node)
    {
        if (node == null) return;
        // Recycle() is obsolete on Android 33+; system manages lifecycle automatically
#pragma warning disable CA1422 // Validate platform compatibility
        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
        {
            node.Recycle();
        }
        else
        {
            node.Dispose();
        }
#pragma warning restore CA1422 // Validate platform compatibility
    }

    private static AccessibilityNodeInfo? ObtainNode(AccessibilityNodeInfo? node)
    {
        if (node == null) return null;
#pragma warning disable CA1422 // Validate platform compatibility
        return AccessibilityNodeInfo.Obtain(node);
#pragma warning restore CA1422 // Validate platform compatibility
    }

    private static List<AccessibilityNodeInfo> FindNodesBySelector(AccessibilityNodeInfo root, string selector)
    {
        var nodes = new List<AccessibilityNodeInfo>();
        if (string.IsNullOrWhiteSpace(selector)) return nodes;

        var textMatch = Regex.Match(selector, @"text='([^']*)'" );
        var descMatch = Regex.Match(selector, @"desc='([^']*)'" );
        var idMatch   = Regex.Match(selector, @"id='([^']*)'"  );

        var queue = new Queue<AccessibilityNodeInfo?>();
        queue.Enqueue(ObtainNode(root));

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node == null) continue;

            bool matches = true;
            if (textMatch.Success && node.Text              != textMatch.Groups[1].Value) matches = false;
            if (descMatch.Success && node.ContentDescription != descMatch.Groups[1].Value) matches = false;
            if (idMatch.Success   && node.ViewIdResourceName  != idMatch.Groups[1].Value)   matches = false;

            if (matches)
            {
                var obtained = ObtainNode(node);
                if (obtained != null) nodes.Add(obtained);
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i);
                queue.Enqueue(child);
            }
            RecycleNode(node);
        }
        return nodes;
    }

    private static AccessibilityNodeInfo? FindRootNode(AccessibilityNodeInfo node)
    {
        var current = ObtainNode(node);
        if (current == null) return null;
        
        while (current.Parent is { } parent)
        {
            RecycleNode(current);
            current = parent;
        }
        return current;
    }

    public override void OnInterrupt() { }

    protected override void OnServiceConnected()
    {
        base.OnServiceConnected();
        SetServiceInfo(new AccessibilityServiceInfo
        {
            EventTypes        = EventTypes.WindowStateChanged | EventTypes.WindowContentChanged,
            FeedbackType      = FeedbackFlags.Generic,
            Flags             = AccessibilityServiceFlags.Default | AccessibilityServiceFlags.RetrieveInteractiveWindows,
            NotificationTimeout = 100,
        });
    }
}
