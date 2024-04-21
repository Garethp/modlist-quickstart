using System.Xml;
using JetBrains.Annotations;

namespace ModlistQuickstart.ModlistManager;

public static class XmlUtils
{
    [CanBeNull]
    public static XmlNode GetChildNodeByName(XmlNode element, string name, int index = -1)
    {
        for (var i = 0; i < element.ChildNodes.Count; i++)
        {
            var childNode = element.ChildNodes.Item(i);
            
            // If the node is an <li> node, the name will always match even if the value is different.
            // For comparing equality or generating diffs, it makes sense to instead take into account position in the list
            if (childNode?.Name == name && (name != "li" || index == i)) return childNode;
        }

        return null;
    }

    public static bool NodesAreEqual(XmlNode expected, XmlNode comparison)
    {
        if (!expected.HasChildNodes && !comparison.HasChildNodes) return expected.Value == comparison.Value;
        if (!expected.HasChildNodes && comparison.HasChildNodes) return false;
        if (expected.HasChildNodes && !comparison.HasChildNodes) return false;

        for (var i = 0; i < expected.ChildNodes.Count; i++)
        {
            var expectedChildNode = expected.ChildNodes.Item(i);
            var compareChildNode = GetChildNodeByName(comparison, expectedChildNode!.Name, i);
            if (compareChildNode is null) return false;

            if (!NodesAreEqual(expectedChildNode, compareChildNode)) return false;
        }

        for (var i = 0; i < comparison.ChildNodes.Count; i++)
        {
            var comparisonChildNode = comparison.ChildNodes.Item(i);
            var expectedChildNode = GetChildNodeByName(expected, comparisonChildNode!.Name, i);
            if (expectedChildNode is null) return false;
        }

        return true;
    }

    [CanBeNull]
    public static XmlNode GenerateDiff(XmlNode expected, XmlNode comparison)
    {
        if (NodesAreEqual(expected, comparison)) return null;

        if (!expected.HasChildNodes && !comparison.HasChildNodes) return comparison.CloneNode(true);
        if (!expected.HasChildNodes && comparison.HasChildNodes) return comparison.CloneNode(true);
        if (expected.HasChildNodes && !comparison.HasChildNodes) return comparison.CloneNode(true);

        var diff = expected.CloneNode(false);
        var doc = diff.OwnerDocument;

        for (var i = 0; i < expected.ChildNodes.Count; i++)
        {
            var expectedChildNode = expected.ChildNodes.Item(i);
            var compareChildNode = GetChildNodeByName(comparison, expectedChildNode!.Name, i);
            if (compareChildNode is null)
            {
                diff.AppendChild(doc.ImportNode(expectedChildNode, true));
                continue;
            }

            var childDiff = GenerateDiff(expectedChildNode, compareChildNode);
            if (childDiff != null) diff.AppendChild(doc.ImportNode(childDiff, true));
        }

        for (var i = 0; i < comparison.ChildNodes.Count; i++)
        {
            var comparisonChildNode = comparison.ChildNodes.Item(i);
            var expectedChildNode = GetChildNodeByName(expected, comparisonChildNode!.Name, i);
            if (expectedChildNode is null)
            {
                diff.AppendChild(doc.ImportNode(comparisonChildNode, true));
            }
        }

        return diff;
    }

    public static XmlNode MergeNodes(XmlNode expected, XmlNode comparison)
    {
        if (NodesAreEqual(expected, comparison)) return comparison;

        if (!expected.HasChildNodes && !comparison.HasChildNodes)
        {
            return expected;
        }

        if (!expected.HasChildNodes && comparison.HasChildNodes)
        {
            return expected;
        }

        if (expected.HasChildNodes && !comparison.HasChildNodes)
        {
            return expected;
        }

        for (var i = 0; i < expected.ChildNodes.Count; i++)
        {
            var expectedChildNode = expected.ChildNodes.Item(i);
            var compareChildNode = GetChildNodeByName(comparison, expectedChildNode!.Name, i);
            if (compareChildNode is null)
            {
                comparison.AppendChild(comparison.OwnerDocument.ImportNode(expectedChildNode, true));
                continue;
            }

            var replacement =
                comparison.OwnerDocument.ImportNode(MergeNodes(expectedChildNode, compareChildNode), true);
            compareChildNode.ParentNode.ReplaceChild(replacement, compareChildNode);
        }

        return comparison;
    }

    /**
     * This differs from NodesAreEqual in that this doesn't check if the two nodes are compatible, but if the two nodes
     * can be merged together. For example, if the expected has a child node that the comparison doesn't, they are compatible
     * because there's no conflict however they're not equal. Likewise, if the comparison has a child node that the expected
     * does not then they still don't conflict however they're not equal.
     */
    public static bool NodesAreCompatible(XmlNode expected, XmlNode comparison)
    {
        if (!expected.HasChildNodes && !comparison.HasChildNodes) return expected.Value == comparison.Value;
        if (!expected.HasChildNodes && comparison.HasChildNodes) return false;
        if (expected.HasChildNodes && !comparison.HasChildNodes) return false;

        for (var i = 0; i < expected.ChildNodes.Count; i++)
        {
            var expectedChildNode = expected.ChildNodes.Item(i);
            var compareChildNode = GetChildNodeByName(comparison, expectedChildNode!.Name, i);
            if (compareChildNode is null) continue;

            if (!NodesAreCompatible(expectedChildNode, compareChildNode)) return false;
        }

        return true;
    }
}