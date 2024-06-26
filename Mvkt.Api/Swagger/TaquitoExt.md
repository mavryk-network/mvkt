MvKT extension for the [Taquito](https://tezostaquito.io/) library can significantly speed up your application by reducing the number of requests made to a Mavryk node. Instead, most of the queries are routed through MvKT API which is much more scalable. The best thing is that you don't have to change anything in your code (almost).

## Installation

```
npm i @tzkt/ext-taquito
```

## Usage

In order to enable routing enable the MvKT extension per each `TezosToolkit` instance created:

```
import { TezosToolkit } from '@taquito/taquito';
import { MvktExtension } from '@tzkt/ext-taquito';

const Mavryk = new TezosToolkit('https://basenet.rpc.mavryk.network');
Mavryk.addExtension(new MvktExtension());
```

MvKT extension will route the following requests:
- `getEntrypoints`
- `getScript`
- `getBalance`
- `getDelegate`
- `getNextProtocol`
- `getProtocolConstants`
- `getStorage`
- `getBlockHash`
- `getBlockLevel`
- `getCounter`
- `getBlockTimestamp`
- `getBigMapValue`
- `isAccountRevealed`
- `getLiveBlocks`

All other requests will be executed against the specified node URI, as usual.

### Changing API endpoint

You may override base URL used by the package in the following manner. This may come useful should you want to make requests to a test network or to your custom server.

```
Mavryk.addExtension(new MvktExtension({url: 'https://api.mavryk.network'}));
```