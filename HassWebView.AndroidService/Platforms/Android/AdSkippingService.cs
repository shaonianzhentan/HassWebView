using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.Accessibility;
using HassWebView.AndroidService.AdSkipping;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
// Add a using alias to resolve the ambiguity between System.Action and Android.Views.Accessibility.Action
using Action = Android.Views.Accessibility.Action;

namespace HassWebView.AndroidService.Platforms.Android
{
    [Service(Label = "HassWebView Ad Skipping Service", Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    public class AdSkippingService : AccessibilityService
    {
        // A static manager to hold the rules, accessible from the service.
        public static GkdRuleManager RuleManager { get; } = new GkdRuleManager();

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            if (e.Source == null || string.IsNullOrEmpty(e.PackageName))
            {
                return;
            }

            var appRules = AdSkippingService.RuleManager.GetRulesForApp(e.PackageName);
            if (appRules == null || !appRules.Groups.Any())
            {
                return;
            }

            // Find the root node to start the search from
            var rootNode = FindRootNode(e.Source);
            if (rootNode == null) return;

            // Iterate through rule groups and rules
            foreach (var group in appRules.Groups)
            {
                foreach (var rule in group.Rules)
                {
                    // Simplified selector for now: only handles text, desc, and id.
                    // Example: "[text='跳过'][id='com.example.app:id/skip_button']"
                    var nodes = FindNodesBySelector(rootNode, rule.Matches);
                    if (nodes.Any())
                    {                        
                        // Perform the click on the first matched node
                        var nodeToClick = nodes.First();
                        // Using the overload with a null Bundle to resolve compiler issues
                        nodeToClick.PerformAction(Action.Click, null);
                        nodeToClick.Recycle(); // Recycle the node after use
                        return; // Action taken, no need to process more rules
                    }
                }
            }
        }

        // This function is a simplified parser for the GKD selector syntax.
        private List<AccessibilityNodeInfo> FindNodesBySelector(AccessibilityNodeInfo root, string selector)
        {
            var nodes = new List<AccessibilityNodeInfo>();
            if (root == null || string.IsNullOrWhiteSpace(selector)) return nodes;

            // Very basic parser for attributes like [text='...'], [desc='...'], [id='...']
            var textMatch = Regex.Match(selector, @"text='([^']*)'");
            var descMatch = Regex.Match(selector, @"desc='([^']*)'");
            var idMatch = Regex.Match(selector, @"id='([^']*)'");

            var queue = new Queue<AccessibilityNodeInfo>();
            queue.Enqueue(root);

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
                    nodes.Add(node);
                } else {
                    node.Recycle(); // Recycle if it doesn't match
                }

                for (int i = 0; i < node.ChildCount; i++)
                {
                    var child = node.GetChild(i);
                    if(child != null) {
                        queue.Enqueue(child);
                    }
                }
            }

            return nodes;
        }


        private AccessibilityNodeInfo FindRootNode(AccessibilityNodeInfo node)
        {
            if(node == null) return null;
            var root = node;
            while (root.Parent != null)
            {
                var parent = root.Parent;
                root.Recycle(); // Recycle the intermediate node
                root = parent;
            }
            return root;
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
