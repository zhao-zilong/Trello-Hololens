# Trello-Hololens
Manipulate trello from Hololens

## Getting Started
For using this code, first of all, put your Trello's token, key and your board name in file Asset/Scripts/Trello.cs. Then you have two 
choices.
### Use this application without access control

Build(File->Build setting) directly the scene Asset/Scenes/trello.unity without including the scene Asset/Scenes/login.unity, run the 
applicatin, it will show your board and all the cards in the board without demanding of authentication.

### Use this application with access control
Apart of this project, you have to build a [API Rest](https://github.com/zhao-zilong/Trello-Hololens-Authentication-Interface) for verifying the Id and password, once you have the API Rest link
Build the application with selecting the two scenes, make sure that login.unity be the first scene, trello.unity be the second scene. Run
the application in Unity editor or in Hololens, It will firstly show a panel which you can fill in your id and password, it will pass to shwo
the board and cards if the id and password are correct.
