window.addEventListener('DOMContentLoaded', () => {
    const currentUserId = parseInt(document.getElementById('current-user-id').getAttribute('value'));
    const usersListElement = document.getElementById('users-list');
    const searchUsersField = document.getElementById('search-users-field') as HTMLInputElement;

    searchUsersField.addEventListener('change', async e => {
        let usersHtml = await hubConnection.invoke('searchUsers', { searchValue: searchUsersField.value }) as String[];
        usersListElement.innerHTML = usersHtml.join('');
    });

    hubConnection.on('newMessage', (data) => {
        usersListElement.insertAdjacentHTML('afterbegin', data.chatPreview.html);

        let newChatPreview = usersListElement.firstElementChild;
        let otherUserId = newChatPreview.getAttribute('data-user-id');

        let oldChatPreview = usersListElement.querySelectorAll(`.chat-preview[data-user-id="${otherUserId}"]`)[1];

        if (oldChatPreview == null)
            return;

        oldChatPreview.remove();

        let newLastMessage = newChatPreview.querySelector('.last-message');
        if (newLastMessage.classList.contains('my-message'))
            return;

        let unviewedMessagesCount = oldChatPreview.querySelector('.counter[data-count]').getAttribute('data-count');
        newChatPreview.querySelector('.counter[data-count]').setAttribute('data-count', (parseInt(unviewedMessagesCount) + 1).toString());

    });

    hubConnection.on('messageViewed', (model) => {
        if (model.viewedByUserId == currentUserId) {
            let chatPreview = usersListElement.querySelector(`.chat-preview[data-user-id="${model.messageAuthorId}"]`);
            let counter = chatPreview.querySelector('.counter[data-count]');
            counter.setAttribute('data-count', model.unviewedMessagesLeft.toString());
            return;
        }
    });

});

//async function init() {
//    let chatsHtml = await hubConnection.invoke('getChats') as String[];

//    const chatsListElement = document.getElementById('users-list');
//    chatsListElement.innerHTML = chatsHtml.join();
//}

//init();