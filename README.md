# Qashless-ISOBridge-Integration
ISOBridge for processing Qashless ATM transactions and for integrating into other APIs
The ISOBridge is an application from converting ISO8583 messages into web API payloads.
This application receives ATM transactions done by scanning QR codes using the bank's mobile app
it looks out for some details including account number and the QR 6 digit code necessary for processing a withdrawal transaction
The bridge converts these messages into a payload
calls the APIs used for debiting and making inquiries
and completes a withdrawal transaction

Please contact me for further updates - richarque@gmail.com
