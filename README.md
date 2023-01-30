# Ubik Authenticator

This API will serve as an authenticator in your future large scale multi-user or multiplayer project. It's scalable and micro-service oriented.

### Why C# ?
It's probably easier to maintain a stack of only one language ! That said, this authenticator must just be deployed, and the language used isn't really that important.

## What does it need ?

This can be used more various scales. If you want only one instance of this, you could just create an instance of it with a basic nginx configuration, and have a local SQLite table. MySQL currently not provided.
For a more larger scale, this Authenticator can be launched with multiple instances in the cloud and use [Redis](https://redis.io/) as a datastore. To summarize, UbikAuthenticator can work as a local tool or a cloud-solution product.

To deploy, just edit `env.properties` to your convenience, build and run the image. You can also create a real container, but this is not a lecture on Docker.
```bash
# Clone the git
git clone https://github.com/jamailun/UbikAuthenticator.git
cd UbikAuthenticator

# 1) Build and run  docker image
docker build -t ubik-authenticator -t Dockerfile .
docker run -it -p 5000:80 ubik-authenticator

# 2) Just start the program with dotnet cli
dotnet run
```

## Advantages list

Why use UbikAuthenticator ?
* Free to use, deploy and modify,
* Allow almost any form of [account structure](https://github.com/jamailun/UbikAuthenticator/wiki/Account-structure): makes it very polyvalent. In general, the main _moto_ of this project is to create a super-customizable authentication solution.

## More information

For more information about UbikAuthenticator, please read detailled articles about [how to install](https://github.com/jamailun/UbikAuthenticator/wiki/Installation-guide) and [how to use](https://github.com/jamailun/UbikAuthenticator/wiki/How-to-use) in the github's wiki.

(General detailled chart in progress.)

### TODO

- Add a way to customize informations returned to server after confirmation
- Add a way to chose to force client to connect to the wanted server and not any other; or even to log in without any specific server.
- Add support for MySQL and an other distributed BDD than Redis.
