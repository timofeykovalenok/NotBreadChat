window.addEventListener('DOMContentLoaded', () => {
    const currentUserId = parseInt(document.getElementById('current-user-id').getAttribute('value'));
    const chatsListElement = document.getElementById('chats-list');
    const usersListElement = document.getElementById('users-list');
    const searchUsersField = document.getElementById('search-users-field') as HTMLInputElement;
    const searchUsersClearButton = document.getElementById('search-users-clear-button') as HTMLButtonElement;

    window.addEventListener('popstate', () => {
        let previousChatAnchor = chatsListElement.querySelector('.chat-preview.active');
        previousChatAnchor?.classList.remove('active');

        let currentChatAnchor = chatsListElement.querySelector(`a.chat-preview[href$="${window.location.pathname}"]`);
        currentChatAnchor?.classList.add('active');
    });

    searchUsersField.addEventListener('input', async () => {
        let searchValue = searchUsersField.value.trim();
        if (searchValue == '') {
            hideSearchUsers();
            return;
        }

        chatsListElement.classList.add('d-none');
        let usersHtml = await hubConnection.invoke('searchUsers', { searchValue: searchValue }) as String[];
        usersListElement.innerHTML = usersHtml.join('');
    });

    searchUsersClearButton.addEventListener('click', () => {
        searchUsersField.value = '';
        hideSearchUsers();
    });

    function hideSearchUsers() {
        usersListElement.innerHTML = '';
        chatsListElement.classList.remove('d-none');
    }

    hubConnection.on('newMessage', (data) => {
        chatsListElement.insertAdjacentHTML('afterbegin', data.chatPreview.html);

        let newChatPreview = chatsListElement.firstElementChild;
        formatDatesLocally(newChatPreview);

        let otherUserId = newChatPreview.getAttribute('data-user-id');

        let oldChatPreview = chatsListElement.querySelectorAll(`.chat-preview[data-user-id="${otherUserId}"]`)[1];

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
            let chatPreview = chatsListElement.querySelector(`.chat-preview[data-user-id="${model.messageAuthorId}"]`);
            let counter = chatPreview.querySelector('.counter[data-count]');
            counter.setAttribute('data-count', model.unviewedMessagesLeft.toString());
            return;
        }
    });

});

function formatDatesLocally(element: Element) {
    let timeElements = element.getElementsByTagName('time');

    let dateTimeFormat = Intl.DateTimeFormat(undefined, {
        dateStyle: 'short',
        timeStyle: 'short'
    });

    for (let i = 0; i < timeElements.length; i++) {
        let date = Date.parse(timeElements[i].dateTime);
        timeElements[i].innerText = dateTimeFormat.format(date).replace(',', '');
    }
}

//async function init() {
//    let chatsHtml = await hubConnection.invoke('getChats') as String[];

//    const chatsListElement = document.getElementById('users-list');
//    chatsListElement.innerHTML = chatsHtml.join();
//}

//init();