namespace AhoCorasick;

public class AhoCorasickMatch<T>(int index, string pattern, T value) : IAhoCorasickMatch where T : struct, Enum
{
    public int Index { get; } = index;
    public string Pattern { get; } = pattern;
    public T Value { get; } = value;
    object IAhoCorasickMatch.Value => Value;
}

public class AhoCorasickMatch : IAhoCorasickMatch
{
    public int Index { get; }
    public string Pattern { get; }
    public int Value { get; }
    object IAhoCorasickMatch.Value => Value;

    public AhoCorasickMatch(int index, string pattern, int value)
    {
        Index = index;
        Pattern = pattern;
        Value = value;
    }
}

public interface IAhoCorasickMatch
{
    public int Index { get; }
    public string Pattern { get; }
    public object Value { get; }
}

public interface IAhoCorasick
{
    public IEnumerable<IAhoCorasickMatch> Search(string text);
    public List<IAhoCorasickMatch> SearchAll(string text);
}

public class AhoCorasick<TValue> : IAhoCorasick where TValue : struct, Enum
{
    class Node
    {
        public readonly Dictionary<char, Node> Children = [];
        public readonly List<(string Pattern, TValue Value)> Outputs = [];
        public Node? Failure;
    }

    readonly Node root = new Node();

    public AhoCorasick(IDictionary<string, TValue> patterns)
    {
        foreach ((string pattern, TValue value) in patterns)
        {
            Node node = root;
            
            foreach (char c in pattern)
            {
                if (!node.Children.TryGetValue(c, out Node? child))
                {
                    child = new Node();
                    node.Children[c] = child;
                }

                node = child;
            }

            node.Outputs.Add((pattern, value));
        }

        Queue<Node> queue = [];
        
        foreach (Node child in root.Children.Values)
        {
            child.Failure = root;
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach ((char c, Node child) in current.Children)
            {
                queue.Enqueue(child);
                Node? failure = current.Failure;
                
                while (failure is not null && !failure.Children.ContainsKey(c))
                {
                    failure = failure.Failure;
                }

                child.Failure = failure?.Children.GetValueOrDefault(c) ?? root;
                child.Outputs.AddRange(child.Failure.Outputs);
            }
        }
    }
    
    IEnumerable<IAhoCorasickMatch> IAhoCorasick.Search(string text)
    {
        return Search(text);
    }

    List<IAhoCorasickMatch> IAhoCorasick.SearchAll(string text)
    {
        return Search(text).ToList<IAhoCorasickMatch>();
    }

    public IEnumerable<AhoCorasickMatch<TValue>> Search(string text)
    {
        Node? node = root;
            
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            while (node != root && !node!.Children.ContainsKey(c))
            {
                node = node.Failure;
            }

            node = node.Children.GetValueOrDefault(c) ?? root;
            
            foreach ((string pattern, TValue value) in node.Outputs)
            {
                yield return new AhoCorasickMatch<TValue>(i - pattern.Length + 1, pattern, value);
            }
        }
    }

    public List<AhoCorasickMatch<TValue>> SearchAll(string text)
    {
        return Search(text).ToList();
    }
}

public class AhoCorasick : IAhoCorasick
{
    class Node
    {
        public readonly Dictionary<char, Node> Children = [];
        public readonly List<(string Pattern, int Value)> Outputs = [];
        public Node? Failure;
    }

    readonly Node root = new Node();

    public AhoCorasick(IDictionary<string, int> patterns)
    {
        BuildTrie(patterns);
        BuildFailureAndDictionaryLinks();
    }

    public AhoCorasick(IEnumerable<string> patterns)
    {
        List<string> enumerated = patterns.ToList();
        Dictionary<string, int> dictionary = enumerated.Select((pattern, index) => new { pattern, index }).ToDictionary(x => x.pattern, x => x.index);
        BuildTrie(dictionary);
        BuildFailureAndDictionaryLinks();
    }

    private void BuildTrie(IDictionary<string, int> patterns)
    {
        foreach ((string pattern, int value) in patterns)
        {
            Node node = root;
            
            foreach (char c in pattern)
            {
                if (!node.Children.TryGetValue(c, out Node? child))
                {
                    child = new Node();
                    node.Children[c] = child;
                }

                node = child;
            }

            node.Outputs.Add((pattern, value));
        }
    }

    private void BuildFailureAndDictionaryLinks()
    {
        Queue<Node> queue = [];
        
        foreach (Node child in root.Children.Values)
        {
            child.Failure = root;
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach ((char c, Node child) in current.Children)
            {
                queue.Enqueue(child);
                Node? failure = current.Failure;
                
                while (failure is not null && !failure.Children.ContainsKey(c))
                {
                    failure = failure.Failure;
                }

                child.Failure = failure?.Children.GetValueOrDefault(c) ?? root;
                child.Outputs.AddRange(child.Failure.Outputs);
            }
        }
    }
    
    public IEnumerable<IAhoCorasickMatch> Search(string text)
    {
        Node? node = root;
            
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            while (node != root && !node!.Children.ContainsKey(c))
            {
                node = node.Failure;
            }

            node = node.Children.GetValueOrDefault(c) ?? root;
            
            foreach ((string pattern, int value) in node.Outputs)
            {
                yield return new AhoCorasickMatch(i - pattern.Length + 1, pattern, value);
            }
        }
    }

    public List<IAhoCorasickMatch> SearchAll(string text)
    {
        return Search(text).ToList();
    }
}