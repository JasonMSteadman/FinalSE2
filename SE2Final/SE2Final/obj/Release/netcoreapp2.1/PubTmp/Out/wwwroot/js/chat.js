"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/server").build();


connection.start().catch(function (err) {
    return console.error(err.toString());
});

//  Calls server to populate group box
setTimeout(function () {
    document.getElementById("pagebody").style.display = "flex";
    document.getElementById("dbox").disabled = true;

    connection.invoke("LoadGroupBox").catch(function (err) {
        return console.error(err.toString());
    });
}, 800);

//  Used to populate page after a group have been selected
function groupBoxClose() {
    var body = document.getElementById("pagebody");

    //  Clear old docs
    while (body.firstChild) {
        body.removeChild(body.firstChild);
    }

    var group = document.getElementById("groupbox");
    var selected = group.options[group.selectedIndex].value;

    connection.invoke("FirstContact", selected).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault(); 
}

//  Populate page on load   
connection.on("LoadPage", function (id, title, Docbody, lock, groupName) {
    if (!sameGroup(groupName)) {
        return;
    }

    var div = document.getElementById("pagebody");
 
    //  Create basic doc layout
    var doc = document.createElement("div");
    doc.id = "doc" + id;
    doc.className = "doc";

    //  title
    var head = document.createElement("H2");
    head.appendChild(document.createTextNode(title));
    head.id = "til" + id;
    head.className = "docTitle";
    doc.appendChild(head);

    //  Edit button
    var btn = document.createElement("input");
    btn.type = "button";
    btn.id = "lck" + id;
    btn.onclick = function() { edit(id); };
    doc.appendChild(btn);

    //  Change title button
    var ntbtn = document.createElement("input");
    ntbtn.type = "button";
    ntbtn.id = "new" + id;
    ntbtn.value = "Edit Title"
    ntbtn.onclick = function () { newTitle(id); };
    doc.appendChild(ntbtn);

    //  Delete button
    var delbtn = document.createElement("input");
    delbtn.type = "button";
    delbtn.id = "del" + id;
    delbtn.value = "Delete"
    delbtn.onclick = function () { deleteDoc(id); };
    doc.appendChild(delbtn);

    //  Separator 
    doc.appendChild(document.createElement("br"));

    //  Body of the document
    var body = document.createElement("textarea");
    body.id = "bdy" + id;
    body.style.width = "100%";
    body.style.height = "80%";
    body.value = Docbody;
    body.onkeyup = function () { bodTyping(id) };
    doc.appendChild(body);

    div.appendChild(doc);
    
    var lockID = "lck" + id;

    if (lock == 1) {
        document.getElementById(lockID).disabled = true;
        document.getElementById(lockID).value = "Locked";
        document.getElementById("new" + id).disabled = true;
        document.getElementById("del" + id).disabled = true;
    }
    else {
        document.getElementById(lockID).disabled = false;
        document.getElementById(lockID).value = "Edit";
        document.getElementById("new" + id).disabled = true;
        document.getElementById("del" + id).disabled = true;
    }
    document.getElementById("bdy" + id).disabled = true;
});

//  Updates the title of a document 
connection.on("updateTitle", function (id, title, groupName) {
    if (!sameGroup(groupName)) {
        return;
    }
    var oldTitle = document.getElementById("til" + id);
    oldTitle.innerText = title;
    
});

//  Deletes doc     
connection.on("deletingDoc", function (id, groupName) {
    if (sameGroup(groupName)) {
        document.getElementById("pagebody").removeChild(document.getElementById("doc" + id));
    }
});

//  Used to load group box
connection.on("PopGroup", function (name, selected) {
    var newGroup = document.createElement("option");
    newGroup.value = name;
    newGroup.innerText = name;
    newGroup.id = name;
    document.getElementById("groupbox").appendChild(newGroup);
    if (selected) {
        var group = document.getElementById("groupbox");
        for (var i = 0; i < group.length; ++i) {
            if (group.options[i].value == name) {
                group.selectedIndex = i;
                break;
            }
        }
        groupBoxClose();
    }

});

//  Delete group        
connection.on("removeGroup", function (groupName) {
    var groupbox = document.getElementById("groupbox");

    //  If you have the group selected
    if (sameGroup(groupName)) {
        var group = document.getElementById("pagebody");
        while (group.firstChild) {
            group.removeChild(group.firstChild);
        }

        //  Find and remove group
        for (var i = 0; i < groupbox.length; ++i) {
            if (groupbox.options[i].value == groupName) {
                groupbox.remove(i);
            }
        }

        //  Load new group
        group.selectedIndex = group.childElementCount - 1;
        groupBoxClose();
        return;
    }

    //  Find and remove group
    for (var i = 0; i < groupbox.length; ++i)
    {
        if (groupbox.options[i].value == groupName) {
            groupbox.remove(i);
        }
    }

});

//  Update locks    
connection.on("LockUpdate", function (id, lockState, groupName) {
    if (!sameGroup(groupName)) {
        return;
    }

    if (lockState == 2) {
        document.getElementById("lck" + id).disabled = false;
        document.getElementById("new" + id).disabled = false;
        document.getElementById("del" + id).disabled = false;
        document.getElementById("bdy" + id).disabled = false;
        document.getElementById("lck" + id).value = "Done";

        //  Focus assist
        document.getElementById("bdy" + id).focus();
    }
    else if (lockState == 1) {
        document.getElementById("lck" + id).value = "Locked";
        document.getElementById("lck" + id).disabled = true;
        document.getElementById("new" + id).disabled = true;
        document.getElementById("del" + id).disabled = true;
        document.getElementById("bdy" + id).disabled = true;
    }
    else {
        document.getElementById("lck" + id).disabled = false;
        document.getElementById("new" + id).disabled = true;
        document.getElementById("del" + id).disabled = true;
        document.getElementById("bdy" + id).disabled = true;
        document.getElementById("lck" + id).value = "Edit";
    }

});

//  Updates doc body    
connection.on("UpdateBody", function (textID, body, groupName) {
    if (sameGroup(groupName)) {
        document.getElementById(textID).value = body;
    }
    event.preventDefault(); 
});

//  Obtains the edit lock for a specific document   
function edit(lockID) {
    var lockState = document.getElementById("lck" + lockID).value;

    var group = document.getElementById("groupbox");
    var selected = group.options[group.selectedIndex].value;

    connection.invoke("docLock", lockID, lockState, selected).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

//  sends key strokes to server     
function bodTyping(id) {
    var group = document.getElementById("groupbox");
    var selected = group.options[group.selectedIndex].value;

    var body = document.getElementById("bdy" + id).value;
    connection.invoke("BodyTyping", id, body, selected).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}

//  Creates a new doc   
function newDoc() {
    var group = document.getElementById("groupbox");
    var selected = group.options[group.selectedIndex].value;

    //  Used to ensure a group is selected
    if (group.selectedIndex <= 0) {
        return;
    }

    var name = window.prompt("Please enter your new documents name.");

    //  No name or canceled
    if (name == "" || name == null) {
        return;
    }

    connection.invoke("CreateDoc", name, selected).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault(); 
}

//  Creates a new group and places it in the drop box
function newGroup() {
    var name = window.prompt("Please enter your new group name.");

    //  No name or canceled
    if (name == "" || name == null) {
        return;
    }

    connection.invoke("CreateGroup", name).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault(); 
}

//  Deletes a group and removes it from the drop box
function deleteGroup() {
    var group = document.getElementById("groupbox");
    var selected = group.options[group.selectedIndex].value;

    //  Used to ensure a group is selected
    if (group.selectedIndex <= 0) {
        return;
    }

    var chose = window.confirm("You are about to delete the selected group");

    if (chose) {
        connection.invoke("DeleteGroup", selected).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    }
}

//  Change the title of a doc. (Must hold the lock to use this) 
function newTitle(id) {
    var name = window.prompt("Please enter a new title.");

    //  No name or canceled
    if (name == "" || name == null) {
        return;
    }

    var group = document.getElementById("groupbox");
    var selected = group.options[group.selectedIndex].value;

    connection.invoke("NewTitle", id, name, selected).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault(); 
}

//  Deletes a doc. (Must hold the lock to use this) 
function deleteDoc(id) {
    var chose = window.confirm("You are about to delete the selected document.");

    if (chose) {
        var group = document.getElementById("groupbox");
        var selected = group.options[group.selectedIndex].value;

        connection.invoke("DeleteDoc", id, selected).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    }
}

//  Check user group
function sameGroup(groupName) {
    var group = document.getElementById("groupbox");
    var selected = group.options[group.selectedIndex].value;

    //  Check to make sure the user has the same group open
    if (groupName != selected) {
        return false;
    }
    return true;
}

////// TODO //////////////////////// FIX ME//////////////////////////
//  Used to return user errors from the server (e.g. name in use)
connection.on("error", function (message) {
    alert(message);
    event.preventDefault();
});