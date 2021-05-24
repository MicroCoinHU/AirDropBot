/* 
 * MicroCoin
 *
 * # MicroCoin API  MicroCoin Rider is an API server for the MicroCoin ecosystem.  It acts as the interface between MicroCoin network and applications that want to access the MicroCoin network.  It allows you to submit transactions to the network, check the status of accounts, subscribe to transactions, etc.  Rider provides a RESTful API to allow client applications to interact with the MicroCoin network.  You can communicate with Rider using cURL or just your web browser. However, if you’re building a client application, you’ll likely want to use a MicroCoin SDK in the language of your client\";    # Before you begin  Before you start developing useful to download the MicroCoin wallet. You can download the latest version from  the official [MicroCoin website](https://microcoin.hu)    ## Supported programming languages  MicroCoin Rider is a simple REST API. You can call using any language what you prefer.  At this time we are offering PHP and Javascript SDKs, but you can generate your own using the [swagger codegen](https://github.com/swagger-api/swagger-codegen) project.    ## Networks  We have two networks.  The Mainnet, where the production coin runs and the Testnet where you can develop.  The primary Mainnet endpoint: https://rider.microcoin.hu.  The primary Testnet endpoint: https://testnet.rider.microcoin.hu.    ## Accounts  In other cryptocoins you can generate an \"address\" to receive coins.  In MicroCoin that's not possible, address's are like accounts and accounts are generated by the blockchain.  So, the main difference, is that if you don't have a MicroCoin account (mined by yourself or received from another account's owner) you cannot have receive MicroCoins.    ### How can I receive an account?  An account can only be operated by a private key. Account's owners can change the account's key to a new one.  You can generate a Private/Public key pair. You send the PUBLIC KEY (Note: private key must always be kept private and only for you!) to the owner of an account.  The account's owner changes the key of an account to your new public key.  After this, the owner of the account will be you, and the old owner will not have access to operate with this account because he doesn't know the private key.  **For testing and developing you can use the Testnet. On the Testnet you can easily mining accounts.**    # PHP SDK quickstart guide    ## System requirements:  * PHP 5.5 or later  * Network connection    ## Download the client SDK  First you need a MicroCoin Client PHP SDK.    You can install with composer  ```  composer require microcoin/microcoin  ```  You can download it from [here](https://github.com/MicroCoinHU/MicroCoin-PHP-SDK/releases), or clone from our [Github](https://github.com/MicroCoinHU/MicroCoin-PHP-SDK) repository.  ```bash  git clone https://github.com/MicroCoinHU/MicroCoin-PHP-SDK.git  # install PHP dependencies  composer install  ```  ## Keys and signatures  MicroCoin works with ECDSA signatures, so you need to work with ECDSA keys and signatures.  You can use your favorite **ECDSA** package, or use `simplito/elliptic-php`. We are using `simplito/elliptic-php` in our demos.  You can find a detailed documentation on the official github page: https://github.com/simplito/elliptic-php    ## Generate new ECDSA keyPair  If you have no keys, you must generate a new key, then store it in a secure place.  **Please note: if you lose your key, you lose your accounts and your coins**  ```php  use Elliptic\\EC;  $ec = new EC('secp256k1');  $myKey = $ec->genKeyPair();  ```  ## Import ECDSA private key  If you have a key, you can import it from a hexadecimal string.  ```php  use Elliptic\\EC;  $ec = new EC('secp256k1');  $myKey = $ec->keyFromPrivate(\"PRIVATE KEY IN HEX\");  ```  ### Where is your private key?    ![Private key](/img/privkey.png)    ## List your accounts  If you have accounts you can list there. First time you have no accounts, so you need get one.  Every account belongs to a public key. One public key can be used for multiple accounts.  ```php  $api = new \\MicroCoin\\Api\\AccountApi();  // You must convert the ECPoint to a MicroCoin SimpleKey  $key = new \\MicroCoin\\Model\\SimpleKey();  $key->setCurveType(\"secp256k1\");  $key->setX($myKey->getPublic()->getX()->toString(16));  $key->setY($myKey->getPublic()->getY()->toString(16));    print_r($api->myAccounts($key));  ```  ## Get single account  You can fetch any account if you know the account number  ```php  $api = new \\MicroCoin\\Api\\AccountApi();  print_r($api->getAccount('0-10'));  ```      ## List accounts for sale  You can purchase accounts, but you need to know which accounts are for sale.  ```php  $api = new \\MicroCoin\\Api\\AccountApi();  print_r($api->getOffers());  ```    ## Purchase account  You can purchase any account for sale, if you have enough coins.  ```php  $api = new \\MicroCoin\\Api\\AccountApi();    // First convert your public key  $key = new \\MicroCoin\\Model\\SimpleKey([      \"curve_type\" => \"secp256k1\",      \"x\" => $myKey->getPublic()->getX()->toString(16),      \"y\" => $myKey->getPublic()->getY()->toString(16)  ]);    $purchaseAccountRequest = new \\MicroCoin\\Model\\PurchaseAccountRequest();   // Account to purchase  $purchaseAccountRequest->setAccountNumber('0-10');  // Optional fee  $purchaseAccountRequest->setFee(0);  // This account will pay the price and the fee  $purchaseAccountRequest->setFounderAccount('1-22');    // The new owner  $purchaseAccountRequest->setNewKey($key);   // preapare transaction  $transaction = $api->startPurchaseAccount($purchaseAccountRequest);   // Sign transaction  $sign = $myKey->sign($transaction->getHash());  // Fill signature  $transaction->setSignature(new \\MicroCoin\\Model\\Signature([\"r\"=>$sign->r->toString(16), \"s\"=>$sign->r->toString(16)]));  // Commit transaction  $result = $api->commitPurchaseAccount($transaction);  // You are done  print_r($result);    ```    ## Sending coins  If you have enough balance, you can send coins from your accounts to any valid account.  ```php  $api = new \\MicroCoin\\Api\\TransactionApi();  $sendCoinRequest = new \\MicroCoin\\Model\\TransactionRequest();  // Source account  $sendCoinRequest->setSender('0-10');  // Destination account  $sendCoinRequest->setTarget('1-22');  // Amount to send  $sendCoinRequest->setAmount(0.0001);  // Optinal fee  $sendCoinRequest->setFee(0);  // Optional payload  $sendCoinRequest->setPayload(\"Hello MicroCoin\");  // Prepare a new transaction  $transaction = $api->startTransaction($sendCoinRequest);  // Sign transaction with your private key (private key what belongs to the sender account)  $sign = $myKey->sign($transaction->getHash());  // Fill signature  $transaction->setSignature( new \\MicroCoin\\Model\\Signature([\"r\"=>$sign->r->toString(16), \"s\"=>$sign->r->toString(16)]) );  // Commit transaction  $result = $api->commitTransaction($transaction);  // Coins sent  print_r($result);  ```  ## Change account owner  If you want change your account owner, you can do it with change the assigned public key.  ```php  $changeKeyRequest->setAccountNumber('0-10');  // Key of the new owner  $changeKeyRequest->setNewOwnerPublicKey(new \\MicroCoin\\Model\\SimpleKey([      \"curve_type\" => \"secp256k1\",      \"x\" => $newKey->getPublic()->getX()->toString(16),      \"y\" => $newKey->getPublic()->getY()->toString(16)  ]));  $changeKeyRequest->setFee(0);  // Prepare transaction  $transaction = $api->startChangeKey($changeKeyRequest);  // Fill signature  $transaction->setSignature( new \\MicroCoin\\Model\\Signature([\"r\"=>$sign->r->toString(16), \"s\"=>$sign->r->toString(16)]) );  // Commit transaction  $result = $api->commitPurchaseAccount($transaction);  // You are done  print_r($result);  ```  ## Transaction history  You can fetch the transaction history of any account  ```js  $api = new \\MicroCoin\\Api\\AccountApi();  print_r($api.getTransactions(\"0-10\"));  ````    ***      # JS SDK quickstart guide    ## Download the client SDK  First you need a MicroCoin client SDK.  You can download it from [here](https://github.com/MicroCoinHU/MicroCoin-Javacript-SDK/releases/), or clone from our [Github](https://github.com/MicroCoinHU/MicroCoin-Javacript-SDK) repository.  ```bash  git clone https://github.com/MicroCoinHU/MicroCoin-Javacript-SDK.git  npm install  npm i elliptic  ```  Or in the dist folder you will find the precompiled, production ready files. Copy this files into your project and start working.    ## Keys and signatures  MicroCoin works with ECDSA signatures, so you need to work with ECDSA keys and signatures.  You can use your favorite **ECDSA** package, or use `elliptic.js`. We are using `elliptic.js` in our demos.  You can find a detailed documentation on the official github page https://github.com/indutny/elliptic    ## HTML boilerplate  ```html  <!DOCTYPE html>  <html lang=\"en\">  <head>      <meta charset=\"UTF-8\">      <title>MicroCoin client minimum project</title>      <script src=\"/dist/microcoin-promise.js\"></script>      <script src=\"/dist/elliptic.js\"></script>  </head>  <body>  <script>      var api = new MicroCoin.AccountApi();      api.getAccount(\"0-10\").then(account => console.log(account));  </script>  </body>  </html>  ```    ## Generate new ECDSA keyPair  If you have no keys, you must generate a new key, then store it in a secure place.  **Please note: if you lose your key, you lose your accounts and your coins**  ```js  var ec = new elliptic.ec(\"secp256k1\");  var myKey = ec.genKeyPair();  ```  ## Import ECDSA private key  If you have a key, you can import it from a hexadecimal string.  ```js  var ec = new elliptic.ec(\"secp256k1\");  var myKey = ec.keyPair({ \"priv\":\"PRIVATE KEY IN HEX\", \"privEnc\":\"hex\" });  ```    ### Where is your private key?    ![Private key](/img/privkey.png)      ## Export ECDSA key  Sometimes you need save your key, or you need to display it. You can export your key using this method  ```js  var exportedKey = {     private: keyPair.getPrivate(\"hex\"),     public: {         X: keyPair.getPublic().x.toString(16),         Y: keyPair.getPublic().y.toString(16)      }  }  ```    ## List your accounts  If you have accounts you can list there. First time you have no accounts, so you need get one.  Every account belongs to a public key. One public key can be used for multiple accounts.  ```js  var accountApi = new MicroCoin.AccountApi();  // Never send your private key!  accountApi.myAccounts({      \"curveType\":\"secp256k1\",      \"x\": myKey.getPublic().getX(\"hex\"),      \"y\": myKey.getPublic().getY(\"hex\")  }).then(myAccounts => console.log(myAccounts));  ```    ## Get single account  You can request information from a single account. You can see the balance, name, etc..  ```js  var accountApi = new MicroCoin.AccountApi();  accountApi.getAccount(\"0-10\").then(account => console.log(account));  ```    ## List accounts for sale  You can purchase accounts, but you need to know which accounts are for sale.  ```js  var accountApi = new MicroCoin.AccountApi();  accountApi.getOffers().then(offers => console.log(offers));  ```  ## Purchase account  You can purchase any account for sale, if you have enough coins.  ```js  var accountApi = new MicroCoin.AccountApi();  var purchaseRequest = new MicroCoin.PurchaseAccountRequest();  purchaseRequest.setAccountNumber(\"34689-25\"); // The account to purchase  purchaseRequest.setFounderAccount(\"1-22\");   // The founder account will pay for the account  purchaseRequest.setFee(0);  // Optional miner fee  // This is key of the new owner. You can use your own key, or any key what you want.  // After the purchase completed you can only manage this account with this keyPair  purchaseRequest.setNewKey({      \"CurveType\":\"secp256k1\",      \"X\": myKey.getPublic().getX(\"hex\"),      \"Y\": myKey.getPublic().getY(\"hex\")  });  // First prepare the transaction  accountApi.startPurchaseAccount(purchaseRequest).then(function (transaction) {      // Now we need to sign our transaction using the founder account private key      var signature = myKey.sign(transaction.getHash());      // Now fill the signature property      transaction.signature = { \"r\": signature.r, \"s\": signature.s };      // Last we need to commit our transaction and we are done      accountApi.commitPurchaseAccount(transaction).then((response)=>console.log(response), e => console.error(e));  });  ```    ## Sending coins  If you have enough balance, you can send coins from your accounts to any valid account.  ```js  var transactionApi = new MicroCoin.TransactionApi();  var sendCoinRequest = new MicroCoin.TransactionRequest();  sendCoinRequest.setSender('0-10'); // Source account  sendCoinRequest.setTarget('1-22'); // Target account  sendCoinRequest.setAmount(0.0001); // Amount to send  sendCoinRequest.setFee(0); // optional miner fee, one transaction / block (5 min) is free  sendCoinRequest.setPayload(\"Hello MicroCoin\"); // optional payload  // First we are creating a transaction  transactionApi.startTransaction(sendCoinRequest).then(function (transaction) {      // When the transaction created, we need to sign the transaction      var signature = myKey.sign(transaction.getHash());      // Now fill the signature property      transaction.signature = { \"r\": signature.r, \"s\": signature.s };      // Last we need to commit our transaction and we are done      transactionApi.commitTransaction(transaction).then((response)=>console.log(response), e => console.error(e));  });  ```    ## Change account owner  If you want change your account owner, you can do it with change the assigned public key.  ```js  var accountApi = new MicroCoin.AccountApi();  var request = new MicroCoin.ChangeKeyRequest();  request.setAccountNumber(\"0-10\"); // The account to change  // newKey: Public key of the new owner  request.setNewOwnerPublicKey({      \"curveType\":\"secp256k1\",      \"x\": newKey.getPublic().getX(\"hex\"),      \"y\": newKey.getPublic().getY(\"hex\")  });  // First we are creating a transaction  accountApi.startChangeKey(request).then(function (transaction) {      // When the transaction created, we need to sign the transaction      // myKey: key of the current owner       var signature = myKey.sign(transaction.getHash());      transaction.signature = { \"r\": signature.r, \"s\": signature.s };      // Last we need to commit our transaction and we are done, the new owner can use his/her account      accountApi.commitChangeKey(transaction).then((response)=>console.log(response), e => console.error(e));  });  ```    ## Transaction history  You can fetch the transaction history of any account  ```php  var accountApi = new MicroCoin.AccountApi();  api.getTransactions(\"0-10\").then(history=>console.log(history));  ```
 *
 * OpenAPI spec version: 1.0.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */


using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MicroCoin.API.Client
{
    /// <summary>
    /// <see cref="GlobalConfiguration"/> provides a compile-time extension point for globally configuring
    /// API Clients.
    /// </summary>
    /// <remarks>
    /// A customized implementation via partial class may reside in another file and may
    /// be excluded from automatic generation via a .swagger-codegen-ignore file.
    /// </remarks>
    public partial class GlobalConfiguration : Configuration
    {

    }
}