
# Blazor Without Interop - BWI

This is a stupid test of a really stupid hack to run JavaScript from Blazor without using JSInterop.
There is no real purpose and probably no real use case.
I am not promoting or endorsing this method, but I do like it ***just for fun***.

Changelog:

#### Version 0.1.0-beta-1
- Initial commit contains a 3 project solution with four samples of BWI.
1. Javascript test runner - enter and run any old JavaScript from the Blazor app.
2. File Upload test - Hide the standard file input and use a custom button.
3. Local Storage test - Read/Write local storage with inbuilt JSON serialisation.
4. Form Validation test - Following up a blog post by @shawty an example of html5 form validation


Projects in this repo:

## BlazorWithoutInteropCore

This is the library that provides all the pages and components for the demo.
Everything interesting is in this library.

#### Sample components without JSInterop

- JSRunner.razor - JS component
- FileUploader.razor - Alternative file upload - triggered by your own button.
- Storage.razor - Read/Write localStorage

## BlazorWithoutInterop

A Blazor/WASM front end for the demo - this is a shell only.

Useful for testing everything works in WASM mode.

## BlazorWithoutInteropServer

A Blazor Server side front end for the demo - this is a shell only.

Useful for debugging and making sure everything works server side.

# What On Earth Did You Do?

Well, I was bored and I wanted to run some JavaScript but didn't want the bother of wondering if I was Pre-Rendering or not.

So, I decided to find a way to trigger JavaScript without JSInterop and came up with the idea
"What if there was a way to render an html element and hook into it's lifecycle events to run some JS?"
I mean it's pretty standard practice to do things "onload" in the web world, but I wanted to be able to do this dynamically and **script** tags just didn't suit what I wanted.

After a bit of research, I thought about `img` tags - maybe I could change the source of one of these and have that trigger something.

So, I tried it with two images, swapping between them and it worked - I could hook into the events for the image loading and run my JS snippets.

```
<img src="img1.png" onload="console.log('it works!')"/>

<img src="img2.png" onload="console.log('it works!')"/>
```

But that seems wasteful, loading images just to trigger a script, so I tried toggling an invalid src and hooking into the `onerror` event.

```
<img src="foo" onerror="console.log('it works!')"/>

<img src="bar" onerror="console.log('it works!')"/>
```

This sort of worked - in Chrome, but threw console errors as well - not nice.

A bit more research and I found out that you don't get errors thrown if the URI is invalid, like `//:0`

```
<img src="//:0" onerror="console.log('it works!')"/>
```

The `onerror` event still fires, but it doesn't throw to the console!

Then along came FireFox - Computer says No!

A bit more fiddling and it turns out that FireFox doesn't trigger `onerror` but it does still fire `onloadstart`

So, now, I have this

```
<img src="@($"//:0/{Src}")" onerror="@jsToRun" onloadstart="@jsToRun" hidden/>
```

All I have to do to run arbitrary javascript is set the Src to a unique string, set jsToRun to contain my JavaScript and render the component.

You will find the finished component in `Components/DragonsBeHere/JSRunner.razor`

In the final version, I added a hidden input that can receive a value back from the JavaScript.

```
<input id="@ReturnValueID" type="text" bind="@ReturnValue" hidden/>
```

There is a method on the component to request a value from the script, which wraps your script in a little bit of code to get the return value, stores it in the hidden input and then triggers a change event on the input, so that Blazor's binding is triggered.

```
public void GetValueJS(string js)
{
	jsToRun = $"{ReturnValueID}.value = eval('{js ?? jsToRun}');" +
		$"{ReturnValueID}.dispatchEvent(new Event('change'));";
	Src = DateTime.UtcNow.ToFileTime().ToString();
	Task.Delay(1);
	StateHasChanged();
}
```

When the value is returned, Blazor binding updates the ReturnValue

```
string ReturnValue
{
	get => returnValue;
	set
	{
		returnValue = value;
		ValueReturned?.Invoke(returnValue);
	}
}
```

And there is an Action Parameter that you can bind to - this raises a notification that you have a result.

```
[Parameter] protected Action<string> ValueReturned { get; set; }
```

### Summary
Fun times, hacking away on Blazor.
Don't do this.

