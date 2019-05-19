var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});
connection.on("ReceiveMessage", function (user, message, conIdFromUser) {
    $(".chat-with-user").html(user);
    $(".chat-with-user").attr('id', conIdFromUser);
    $("#msg-window").append("\n" + message);
});
$(".friend").click(function () {

    var conId = $(this).children()[0].getAttribute('id');
    var friendId = $(this).children()[0].getAttribute('name');

    var userName = $(this).children()[1].innerHTML;
    $(".chat-with-user").html(userName);
    $(".chat-with-user").attr('id', conId);

    $.ajax({
        url: "/Home/GetMsgs?friendId=" + friendId,
        success: function (data) {
            for (var i = 0; i < data.length; i++) {
                $("#msg-window").append("\n" + data[i].message);
            }
        }
    });
});
connection.on("UpdateFriendList", function (userId, conId) {
    $("[name=" + userId + "]").attr('id', conId);
    $("[name=" + userId + "]").css({"color" : "lawngreen"});
});
connection.on("UpdateFriendList2", function (list) {
    var arr = JSON.parse(list);
    for(var i = 0; i < arr.length; i++) {
        var updateId = arr[i].Id;
        $("[name=" + updateId + "]").attr('id', arr[i].ConnectionId);
        $("[name=" + updateId + "]").css({ "color": "lawngreen" });
    }    
});

$("#send-msg").click(function () {
    var msg = $("#new-msg").val();
    $("#new-msg").val('');

    var conId = $(".chat-with-user").attr('id');
    $("#msg-window").append("\n" + msg);
    connection.invoke("SendMessageToUser", conId, msg);
});


