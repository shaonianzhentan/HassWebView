using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.Accessibility;
using HassWebView.AndroidService.AdSkipping;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Action = Android.Views.Accessibility.Action;

namespace HassWebView.AndroidService.AdSkipping
{
    [Service(Label = "HassWebView Ad Skipping Service", Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE", Exported = false)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    public class AdSkippingService : AccessibilityService
    {
        public static GkdRuleManager RuleManager { get; } = new GkdRuleManager();

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            if (e.Source == null || string.IsNullOrEmpty(e.PackageName))
            {
                return;
            }

            var appRules = RuleManager.GetRulesForApp(e.PackageName);
            if (appRules == null || !appRules.Groups.Any())
            {
                e.Source.Recycle();
                return;
            }

            var rootNode = FindRootNode(e.Source); // e.Source will be recycled inside
            if (rootNode == null) return;

            foreach (var group in appRules.Groups)
            {
                foreach (var rule in group.Rules)
                {
                    var nodes = FindNodesBySelector(rootNode, rule.Matches);
                    if (nodes.Any())
                    {                        
                        var nodeToClick = nodes.First();
                        nodeToClick.PerformAction(Action.Click);
                        nodeToClick.Recycle();
                        // Recycle other nodes that were found but not clicked
                        foreach (var node in nodes.Skip(1)) { node.Recycle(); }
                        rootNode.Recycle();
                        return; // Action taken
                    }
                }
            }
            rootNode.Recycle(); // Recycle root if no rules matched
        }

        private List<AccessibilityNodeInfo> FindNodesBySelector(AccessibilityNodeInfo root, string selector)
        {
            var nodes = new List<AccessibilityNodeInfo>();
            if (root == null || string.IsNullOrWhiteSpace(selector)) return nodes;

            var textMatch = Regex.Match(selector, @"text=\'([^\']*)\'");
            var descMatch = Regex.Match(selector, @"desc=\'([^\']*)\'");
            var idMatch = Regex.Match(selector, @"id=\'([^\']*)\'");

            var queue = new Queue<AccessibilityNodeInfo>();
            queue.Enqueue(AccessibilityNodeInfo.Obtain(root)); // Start with a copy

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node == null) continue;

                bool matches = true;
                if (textMatch.Success && node.Text != textMatch.Groups[1].Value) matches = false;
                if (descMatch.Success && node.ContentDescription != descMatch.Groups[1].Value) matches = false;
                if (idMatch.Success && node.ViewIdResourceName != idMatch.Groups[1].Value) matches = false;

                if (matches)
                {
                    nodes.Add(AccessibilityNodeInfo.Obtain(node)); // Add a copy to the list
                }

                for (int i = 0; i < node.ChildCount; i++)
                {
                    var child = node.GetChild(i);
                    if (child != null) 
                    {
                        queue.Enqueue(child); // The queue now owns the child node
                    } else {
                        // It's good practice to check for null children, though GetChild should handle it
                    }
                }
                node.Recycle(); // Recycle the node we processed
            }
            return nodes;
        }

        private AccessibilityNodeInfo FindRootNode(AccessibilityNodeInfo node)
        {
            if(node == null) return null;
            var current = AccessibilityNodeInfo.Obtain(node);
            while (current.Parent != null)
            {
                var parent = current.Parent;
                current.Recycle();
                current = parent;
            }
            return current;
        }

        public override void OnInterrupt() { }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            var serviceInfo = new AccessibilityServiceInfo
            {
                EventTypes = EventTypes.WindowStateChanged | EventTypes.WindowContentChanged,
                FeedbackType = FeedbackFlags.Generic,
                Flags = AccessibilityServiceFlags.Default | AccessibilityServiceFlags.RetrieveWindowContent,
                NotificationTimeout = 100
            };
            SetServiceInfo(serviceInfo);
        }
    }
}
