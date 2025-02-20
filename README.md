# webapi-couchbase
Estudo de utilização do Couchbase como banco de dados.

[Documentação do Couchbase](https://docs.couchbase.com/dotnet-sdk/current/hello-world/start-using-sdk.html)

# Consultas na interface do couchbase
Para consultar e retornar o id junto com o documento
`select META().id AS id, * FROM Store;`