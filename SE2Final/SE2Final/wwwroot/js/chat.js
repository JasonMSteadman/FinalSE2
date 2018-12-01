"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/server").build();


connection.start().catch(function (err) {
    return console.error(err.toString());
});

//  Calls server to populate page
setTimeout(function () {
    document.getElementById("pagebody").style.display = "flex";

    var group = "group one"; //TODO remove hard coding
    connection.invoke("FirstContact", group).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}, 500);

//  Populate page on load ////////////TODO////////////////////////
connection.on("LoadPage", function (id, title, Docbody, lock) {
    var div = document.getElementById("pagebody");
 
    //  Create basic doc layout
    var doc = document.createElement("div");
    doc.id = "doc" + id;
    doc.style.height = "40%";
    doc.style.widgth = "45%";
    doc.style.margin = "5%";
    doc.style.background = "black";


    var head = document.createElement("H1");
    head.appendChild(document.createTextNode(title));
    head.id = "til" + id;
    doc.appendChild(head);

    var btn = document.createElement("input");
    btn.type = "button";
    btn.id = "lck" + id;
    btn.onclick = function() { edit(id); };
    doc.appendChild(btn);

    doc.appendChild(document.createElement("br"));

    var body = document.createElement("textarea");
    body.id = "bdy" + id;
    body.style.width = "*";
    body.value = Docbody;
    body.onkeyup = function () { bodTyping(id) };
    doc.appendChild(body);

    div.appendChild(doc);
    
    var lockID = "lck" + id;

    if (lock == 1) {
        document.getElementById(lockID).contentEditable = "false";
        document.getElementById(lockID).value = "Locked";
    }
    else {
        document.getElementById(lockID).contentEditable = "true";
        document.getElementById(lockID).value = "Edit";
    }
    document.getElementById("bdy" + id).disabled = true;
});

//  TODO remove this    ///////////////////////////////////////////
connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

//  Update locks
connection.on("LockUpdate", function (lockID, docID, lockState) {
    if (lockState == 2) {
        document.getElementById(lockID).contentEditable = "true";
        document.getElementById(docID).disabled = false;
        document.getElementById(lockID).value = "Done";
    }
    else if (lockState == 1) {
        document.getElementById(lockID).contentEditable = "false";
        document.getElementById(docID).disabled = true;
        document.getElementById(lockID).value = "Locked";
    }
    else {
        document.getElementById(lockID).contentEditable = "true";
        document.getElementById(docID).disabled = true;
        document.getElementById(lockID).value = "Edit";
    }

});

//  Updates doc body
connection.on("UpdateBody", function (textID, body) {
    document.getElementById(textID).value =  body;
});

// TODO remove this     /////////////////////////////////
document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//  Obtains the edit lock for a specific document
function edit(lockID) {
    var lockState = document.getElementById("lck" + lockID).value;
    connection.invoke("docLock", lockID, lockState).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

//  sends key strokes to server
function bodTyping(id) {

    var body = document.getElementById("bdy" + id).value;
    connection.invoke("BodyTyping", id, body).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

//  While editing a file
document.getElementsByClassName("file").addEventListener("keypress", function (event) {
    var textID = "txt" + "confirmSubmit(this);"

    var body = getElementById(textID).value;
    //  Create message and title section

    connection.invoke("updateFile", textID, body).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//  Creates a new doc
function newDoc() {
    var name = window.prompt("Please enter your new documents name");

    connection.invoke("CreateDoc", name).catch(function (err) {
        return console.error(err.toString());
    });
}



