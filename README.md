## Token Merchant Checkout Sample: C#

This sample code shows how to enable the Token Merchant Checkout
experience on a simple web server.
You can learn more about the Quick Checkout flow and relevant APIs at the
[Merchant Quick Checkout documentation](https://developer.token.io/merchant-checkout/).

## Requirements

### On Windows

There are no prerequisites for Windows.

### On Linux and OSX

Install `Mono` from [here](https://www.mono-project.com/download/stable/).

 `Mono` is an open source implementation of Microsoft's .NET Framework. It brings the .NET framework to non-Windows envrionments like Linux and OSX.

## Build and Run

To build

``` 
nuget restore

msbuild
```

To run 

```
xsp4 --address=localhost --port=3000
```

This starts up a server.

The first time you run the server, it creates a new Member (Token user account).
It saves the Member's private keys in the `keys` directory.
In subsequent runs, the server uses this ID these keys to log the Member in.

The server operates in Token's Sandbox environment. This testing environment
lets you try out UI and payment flows without moving real money.

The server shows a web page at `localhost:3000`. The page has a checkout button.
Clicking the button starts the Token merchant payment flow.
The server handles endorsed payments by redeeming tokens.

Test by going to `localhost:3000`.
You can't get far until you create a customer member as described at the
[Merchant Quick Checkout documentation](https://developer.token.io/merchant-checkout/).

This code uses a publicly-known developer key (the devKey line in the
InitializeSDK method). This normally works, but don't be surprised if
it's sometimes rate-limited or disabled. If your organization will do
more Token development and doesn't already have a developer key, contact
Token to get one.

### Implementing Cross Border payments

To allow TPPs to make better decision while selecting the destination account,
we have an additional flow available for them.

For information on how to use it, go to the following link

https://developer.token.io/docs/?csharp#cross-border-payments

Note: If SWIFT is used as the transfer destination type and the user selects a UK bank,
then the account number should follow IBAN format.

### Troubleshooting

If anything goes wrong, try to clear your browser's cache before retest.

If you see the following error: `The CodeDom provider type "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider" 
could not be located` try removing the NuGet packages Microsoft.CodeDom.Providers.DotNetCompilerPlatform 
and Microsoft.Net.Compilers as per the advice in https://stackoverflow.com/questions/33319675/the-codedom-provider-type-microsoft-codedom-providers-dotnetcompilerplatform-cs.
