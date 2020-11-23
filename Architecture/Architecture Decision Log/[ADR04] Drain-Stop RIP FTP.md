# UI tests structure

## Status

Proposed

## Context

Due to the poor stability of our UI tests pipelines we decided to try to imporve their structure.

## Decision

Each class that uses WebDriver interacion should have get-only properties with element selector description (the *By* class)):

```cs
protected By VolumePrefixInput => By.Id("volume-prefix-input");
```

To work with those effectively, we also need *Chain* extension method:

```cs
/// <summary>
/// This method allows to chain By instances. It makes the IWebDriver search for the next By inside of the parent, and so on.
/// </summary>
/// <param name="parent">parent</param>
/// <param name="children">selectors for next levels of search</param>
/// <returns></returns>
public static By Chain(this By parent, params By[] children)
{
    return new ByChained(new[] { parent }.Concat(children).ToArray());
}
```

All verbs (like Click, ScrollToView, etc) should be implemented as extension methods on *By* class and use explicit waits and Selenium built in retries. All logic should be embedded in *wait.Unit()* method and return the *By* argument to enable fluent chaining:

```cs
public static By Click(this By by, IWebDriver driver, bool pageShouldChange = false, TimeSpan? timeout = null)
{
    WebDriverWait wait = driver.GetConfiguredWait();

    wait.Until(d =>
    {
        var el = d.FindElement(by);
        string currentUrl = driver.Url;
        el.Click();
        if (pageShouldChange && currentUrl == driver.Url)
        {
            return null;
        }

        return el;
    });

    return by;
}
```


### Componnets

Our existing component library proved to be usefull, so we should recreate them using this new approach.

Base component class:

```cs
public abstract class Component
{
    public IWebDriver Driver { get; }

    protected readonly By Parent;

    protected Component(By parent, IWebDriver driver)
    {
        Driver = driver;
        Parent = parent;
    }
}
```

Sample component:

```cs
public class SampleComponent
{
    public By ComponentElement => Parent.Chain(By.Id("someId"));
    
    public SampleComponent(By parentElementSelector, IWebDriver driver) : base(parentElementSelector, driver)
    {}
}
```

### Page models

Page models should be composed using the components. 

```cs
public class SomePage : Page
{
    protected RadioSelect ModeSelect {get;} = new RadioSelect(By.Id("modeSelect"), Driver);
    
    public string Mode 
    {
        get => ModeSelect.SelectedOption;
        set => ModeSelect.Select(value) ;
    }
}
```

Actions on page that end with transition should return new page model.

## What to test

Current UI test serve more as end-to-end tests and they cover more than they should. New strategy of UI testing should be complemented by moving the test coverage to a new project. 

New UI tests should cover just the UI:

### Configuration:

- Determine which combinations are worth testing
    - For example selecting images when some fields are mapped for sure
    - Creating destination production
    - Fields mappings warnings
- Just save and retrieve IP with API and check if it is saved correctly

### Editing

- Create IP with API
- Check if each step loaded correct values
	
### Fields mapping
- Existing scenarios

	
### Running

- Create IP with API
- Click "Run" and check if it becomes disabled

### Stopping

- Start IP with API
- Check if Stop button is enabled
- Click (SYNC during Sync job, should be long enough)
- Check status of IP with API
- Check status on UI

### Retry

- Create and run IP with API so that it has Item Level Errors
- Check if Retry Errors is enabled
- Click
- Check with API if IP is running in retry mode (if possible)


## Additional resources

- [Einstein article on Selenium tests](https://einstein.kcura.com/display/PT/Management+Console+-+UI+Tests+Guidelines)

## Consequences

+ UI will take much less time
+ UI tests will test just the UI
+ there will be less UI test
+ new layer of tests will enable automatic regression