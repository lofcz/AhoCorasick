namespace AhoCorasick.Tests;

[TestFixture]
public class AhoCorasickTests
{
    enum WordCategory
    {
        Noun,
        Verb,
        Adjective,
        Adverb
    }
    
    Dictionary<string, WordCategory> patterns;
    AhoCorasick<WordCategory> ahoCorasick;

    [SetUp]
    public void Setup()
    {
        patterns = new Dictionary<string, WordCategory>
        {
            {"he", WordCategory.Noun},
            {"she", WordCategory.Noun},
            {"his", WordCategory.Adjective},
            {"hers", WordCategory.Adjective},
            {"run", WordCategory.Verb},
            {"quickly", WordCategory.Adverb}
        };
        ahoCorasick = new AhoCorasick<WordCategory>(patterns);
    }

    [Test]
    public void Search_SingleMatch_ReturnsCorrectResult()
    {
        const string text = "he runs";
        List<AhoCorasickMatch<WordCategory>> results = ahoCorasick.Search(text).ToList();

        Assert.That(results, Has.Count.EqualTo(2));
        
        Assert.Multiple(() =>
        {
            Assert.That(results[0].Index, Is.EqualTo(0));
            Assert.That(results[0].Pattern, Is.EqualTo("he"));
            Assert.That(results[0].Value, Is.EqualTo(WordCategory.Noun));
        });
    }

    [Test]
    public void Search_OverlappingMatches_ReturnsAllMatches()
    {
        string text = "hishers";
        List<AhoCorasickMatch<WordCategory>> results = ahoCorasick.Search(text).ToList();

        Assert.That(results, Has.Count.EqualTo(4));
        
        Assert.That(results[0].Pattern, Is.EqualTo("his"));
        Assert.That(results[0].Value, Is.EqualTo(WordCategory.Adjective));
        
        Assert.That(results[1].Pattern, Is.EqualTo("she"));
        Assert.That(results[1].Value, Is.EqualTo(WordCategory.Noun));
        
        Assert.That(results[2].Pattern, Is.EqualTo("he"));
        Assert.That(results[2].Value, Is.EqualTo(WordCategory.Noun));
        
        Assert.That(results[3].Pattern, Is.EqualTo("hers"));
        Assert.That(results[3].Value, Is.EqualTo(WordCategory.Adjective));
    }

    [Test]
    public void Search_NoMatches_ReturnsEmptyList()
    {
        string text = string.Empty;
        List<AhoCorasickMatch<WordCategory>> results = ahoCorasick.Search(text).ToList();

        Assert.That(results, Is.Empty);
    }
    
    [Test]
    public async Task Search_MultipleThreads_ReturnsCorrectResults()
    {
        AhoCorasick<WordCategory> local = new AhoCorasick<WordCategory>(patterns);
        string[] texts =
        [
            "she runs quickly",
            "he runs his course",
            "hers is quickly done",
            "his run was quick"
        ];

        List<Task<List<AhoCorasickMatch<WordCategory>>>> tasks = texts.Select(text => Task.Run(() => local.Search(text).ToList())).ToList();

        await Task.WhenAll(tasks);
        
        Assert.Multiple(() =>
        {
            Assert.That(tasks[0].Result, Has.Count.EqualTo(4)); // "she runs quickly"
            Assert.That(tasks[1].Result, Has.Count.EqualTo(3)); // "he runs his course"
            Assert.That(tasks[2].Result, Has.Count.EqualTo(3)); // "hers is quickly done"
            Assert.That(tasks[3].Result, Has.Count.EqualTo(2)); // "his run was quick"
        });

        List<AhoCorasickMatch<WordCategory>> firstResults = tasks[0].Result;
        Assert.Multiple(() =>
        {
            Assert.That(firstResults[0].Pattern, Is.EqualTo("she"));
            Assert.That(firstResults[1].Pattern, Is.EqualTo("he"));
            Assert.That(firstResults[2].Pattern, Is.EqualTo("run"));
            Assert.That(firstResults[3].Pattern, Is.EqualTo("quickly"));
        });

        List<AhoCorasickMatch<WordCategory>> thirdResults = tasks[2].Result;
        Assert.Multiple(() =>
        {
            Assert.That(thirdResults[0].Pattern, Is.EqualTo("he"));
            Assert.That(thirdResults[1].Pattern, Is.EqualTo("hers"));
            Assert.That(thirdResults[2].Pattern, Is.EqualTo("quickly"));
        });

        List<AhoCorasickMatch<WordCategory>> fourthResults = tasks[3].Result;
        Assert.Multiple(() =>
        {
            Assert.That(fourthResults[0].Pattern, Is.EqualTo("his"));
            Assert.That(fourthResults[1].Pattern, Is.EqualTo("run"));
        });

        foreach (Task<List<AhoCorasickMatch<WordCategory>>> task in tasks)
        {
            Assert.That(task.IsCompletedSuccessfully, Is.True);
        }
    }
}

