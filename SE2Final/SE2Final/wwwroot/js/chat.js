"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/server").build();

connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//  Obtains the edit lock for a specific document
document.getElementsByClassName("edit").addEventListener("click", function (event) {
    var textID = "confirmSubmit(this);"
    connection.invoke("lock", textID).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//  While editing a file
document.getElementsByClassName("file").addEventListener("keypress", function (event) {
    var textID = "confirmSubmit(this);"
    
    connection.invoke("lock", textID).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//  Send char
//  Edit/unlock
