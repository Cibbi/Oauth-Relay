<div align="center">

# OAuth2 App Relay Server

[![Generic badge](https://img.shields.io/github/downloads/Cibbi/Oauth-relay/total?label=Downloads)](https://github.com/Cibbi/Oauth-relay/releases/latest)
[![Generic badge](https://img.shields.io/badge/License-MIT-informational.svg)](https://github.com/Cibbi/Oauth-relay/blob/main/LICENSE)

REST Api that handles the Oauth authentication process for a client application without leaking the oauth app secret to the client

### ⬇️ [Download latest Release](https://github.com/Cibbi/Oauth-relay/releases/latest)

</div>

---

## How it works

This REST Api provides 2 endpoints: one that gives the URL to start the Oauth process with the specified OAuth provider, and another endpoint that takes in the oauth temp code and uses it to generate the final oauth token.

Here's a simple schema of the process:



The program also provides settings to limit the requests attempts via a secondary api login token of your choice

## Usage


## License

The sofware is available as-is under MIT. For more information see [LICENSE](https://github.com/Cibbi/Oauth-relay/blob/main/LICENSE).
