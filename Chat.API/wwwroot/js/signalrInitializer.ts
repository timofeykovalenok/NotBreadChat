//@ts-nocheck
type _ = typeof import('@microsoft/signalr');

const hubConnection = new window.signalR.HubConnectionBuilder()
    .withUrl("/sockets")
    .withStatefulReconnect()
    .withAutomaticReconnect()
    .build();

window.addEventListener('focus', () => {  
    startHubConnection();
});

startHubConnection();

function startHubConnection() {
    if (hubConnection.state != "Disconnected")
        return;

    hubConnection.start()
        .then(async () => {
            let chatsHtml = await hubConnection.invoke('getChats', {}) as String[];

            const chatsListElement = document.getElementById('users-list');
            chatsListElement.innerHTML = chatsHtml.join("");
        })
        .catch(function (err) {
            return console.error(err.toString());
        });
}