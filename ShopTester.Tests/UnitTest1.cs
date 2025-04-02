using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace ShopTester.Tests;

[TestClass]
public class DemoTest : PageTest
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _browserContext;
    private IPage _page;

    [TestInitialize]
    public async Task Setup()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 1000 // Lägger in en fördröjning så vi kan se vad som händer
        });
        _browserContext = await _browser.NewContextAsync();
        _page = await _browserContext.NewPageAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _browserContext.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [TestMethod]
    public async Task TestProgram()
    {
        CheckDisplayingPages();
        LoginAndLogout();
        OrderProduct();
    }

    [TestMethod]
    public async Task CheckDisplayingPages()
    {
        await _page.GotoAsync("http://localhost:5000/");

        //Kontrollera att Home finns på sidan och att den innehåller texten "Welcome to TestShopper!"
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Welcome to TestShopper!" })).ToBeVisibleAsync();

        // Klicka på länken till "Shop"
        await _page.GetByRole(AriaRole.Link, new() { Name = "Shop" }).ClickAsync();
        // Kontrollera att knappen "Electronics" finns på sidan
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Electronics" })).ToBeVisibleAsync();

        // Checka Cart och att den displayar "Your Cart is Empty"
        await _page.GetByRole(AriaRole.Link, new() { Name = "Cart" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Shopping Cart" })).ToBeVisibleAsync();

        //Check login page and that it contains "submit" button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Submit" })).ToBeVisibleAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();

        //check register and that it contains "submit" button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Submit" })).ToBeVisibleAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Close" }).ClickAsync();
    }

    [TestMethod]
    public async Task LoginAndLogout()
    {
        await _page.GotoAsync("http://localhost:5000/");

        // Klicka på länken till "Login"
        await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        // Kontrollera att knappen "submit" finns på sidan
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Submit" })).ToBeVisibleAsync();

        // Fyll i formuläret och klicka på "Login"
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email:" }).FillAsync("admin@admin.com");
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password:" }).FillAsync("admin123");
        await _page.GetByRole(AriaRole.Button, new() { Name = "submit" }).ClickAsync();

        // Kontrollera att användaren är inloggad
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();

        // Klicka på länken till "Logout"
        await _page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        // Kontrollera att användaren är utloggad
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();
    }
    [TestMethod]
    public async Task OrderProduct()
    {
        await _page.GotoAsync("http://localhost:5000/");
        // Klicka på länken till "Login"
        await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        // Kontrollera att knappen "submit" finns på sidan
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Submit" })).ToBeVisibleAsync();
        // Fyll i formuläret och klicka på "Login"
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Email:" }).FillAsync("admin@admin.com");
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password:" }).FillAsync("admin123");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();


        await _page.GetByRole(AriaRole.Link, new() { Name = "Shop" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Electronics" })).ToBeVisibleAsync();

        //Add specific product
        await _page.Locator("#button-add-smartphone").ClickAsync();

        //Go to cart
        await _page.GetByRole(AriaRole.Link, new() { Name = "Cart" }).ClickAsync();

        //Add more of the same product
        await _page.Locator("#button-add").DblClickAsync();
        await _page.Locator("#button-add").DblClickAsync();
        await _page.Locator("#button-add").DblClickAsync();
        await _page.Locator("#button-add").DblClickAsync();
        await _page.Locator("#button-reduce").DblClickAsync();
        await _page.Locator("#button-reduce").DblClickAsync();

        //Order the product
        await _page.Locator("#checkout-button").ClickAsync();
        await Expect(_page.GetByText("Order placed successfully! Thank you for your purchase.")).ToBeVisibleAsync();
    }
}