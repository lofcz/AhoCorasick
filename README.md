# AhoCorasick

## Install

```
dotnet add package AhoCorasickCore
```

## Use

This implementation of [Aho-Corasick](https://en.wikipedia.org/wiki/Aho%E2%80%93Corasick_algorithm) can be used in scenarios where a string needs to be matched against several substrings, and each substring is assigned a certain meaning. For example, one could scan an e-mail against a few words known to be used by spammers and trigger some follow-up actions on each match. Instead of doing that linearly (e.g., by calling `Contains` on each needle), AhoCorasick and similar algorithms scan efficiently by reusing the already traversed space.

A minimal example:

```cs
enum WordCategory
{
    Noun,
    Verb,
    Adjective,
    Adverb
}

Dictionary<string, WordCategory> patterns = new Dictionary<string, WordCategory>
{
    {"he", WordCategory.Noun},
    {"she", WordCategory.Noun},
    {"his", WordCategory.Adjective},
    {"hers", WordCategory.Adjective},
    {"run", WordCategory.Verb},
    {"quickly", WordCategory.Adverb}
};

// cache the instance and reuse it, all public methods are thread-safe
AhoCorasick inst = new AhoCorasick<WordCategory>(patterns);

// use Search() for consuming hits via yield
List<AhoCorasickMatch<WordCategory>> results = inst.SearchAll("he runs")

/* returns: [
  {pattern: "he", value: (Noun), pos: 0},
  {pattern: "run", value: (Verb), pos: 3}
] */
```
