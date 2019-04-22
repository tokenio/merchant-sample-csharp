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

## Build

To build

``` 
nuget restore

msbuild
```

To run 
```
mono  /Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/xsp4.exe --address=127.0.0.1 --port=5000
```

`Note: the path to xsp4.exe might be different in Linux and Windows`

This starts up a server.

The first time you run the server, it creates a new Member (Token user account).
It saves the Member's private keys in the `keys` directory.
In subsequent runs, the server uses this ID these keys to log the Member in.

The server operates in Token's Sandbox environment. This testing environment
lets you try out UI and payment flows without moving real money.

The server shows a web page at `127.0.0.1:5000`. The page has a checkout button.
Clicking the button starts the Token merchant payment flow.
The server handles endorsed payments by redeeming tokens.

Test by going to `127.0.0.1:5000`.
You can't get far until you create a customer member as described at the
[Merchant Quick Checkout documentation](https://developer.token.io/merchant-checkout/).

This code uses a publicly-known developer key (the devKey line in the
initializeSDK method). This normally works, but don't be surprised if
it's sometimes rate-limited or disabled. If your organization will do
more Token development and doesn't already have a developer key, contact
Token to get one.

### Troubleshooting

If anything goes wrong, try to clear your browser's cache before retest.
