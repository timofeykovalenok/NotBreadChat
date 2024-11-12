window.addEventListener('DOMContentLoaded', () => {
    //#region Fields

    const currentUserId = parseInt(document.getElementById('current-user-id').getAttribute('value'));
    const chatsListElement = document.getElementById('chats-list');
    const usersListElement = document.getElementById('users-list');
    const searchUsersField = document.getElementById('search-users-field') as HTMLInputElement;
    const searchUsersClearButton = document.getElementById('search-users-clear-button') as HTMLButtonElement;

    //#endregion

    window.addEventListener('popstate', onPopState);

    searchUsersField.addEventListener('focus', onSearchUsersFieldFocus);
    searchUsersField.addEventListener('blur', onSearchUsersFieldBlur);
    searchUsersField.addEventListener('input', onSearchUsersFieldInput);
    searchUsersClearButton.addEventListener('click', onSearchUsersClearButtonClick);
    usersListElement.addEventListener('click', onUsersListClick);
    usersListElement.addEventListener('mousedown', e => e.preventDefault());       
    
    hubConnection.on('newMessage', onNewMessageEvent);
    hubConnection.on('messageViewed', onMessageViewed);

    //#region DOM Events

    function onPopState() {
        let previousChatAnchor = chatsListElement.querySelector('.chat-preview.active');
        previousChatAnchor?.classList.remove('active');

        let currentChatAnchor = chatsListElement.querySelector(`a.chat-preview[href$="${window.location.pathname}"]`);
        currentChatAnchor?.classList.add('active');
    }

    function onSearchUsersFieldFocus() {
        searchUsers();
    }

    function onSearchUsersFieldBlur() {
        clearSearchUsers();
    }

    function onSearchUsersFieldInput() {
        if (searchUsersField.value.trim() == '') {
            return;
        }

        searchUsers();
    }

    function onSearchUsersClearButtonClick() {
        searchUsersField.value = '';
        clearSearchUsers();
    }

    function onUsersListClick() {
        searchUsersField.blur();
        clearSearchUsers();
    }

    //#endregion

    //#region Hub Events

    function onNewMessageEvent(data) {
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
    }

    function onMessageViewed(model) {
        if (model.viewedByUserId == currentUserId) {
            let chatPreview = chatsListElement.querySelector(`.chat-preview[data-user-id="${model.messageAuthorId}"]`);
            let counter = chatPreview.querySelector('.counter[data-count]');
            counter.setAttribute('data-count', model.unviewedMessagesLeft.toString());
            return;
        }
    }

    //#endregion

    //#region Private Methods

    async function searchUsers() {
        let searchValue = searchUsersField.value.trim();
        chatsListElement.classList.add('d-none');
        let usersHtml = await hubConnection.invoke('searchUsers', { searchValue: searchValue }) as String[];
        usersListElement.innerHTML = usersHtml.join('');
    }

    function clearSearchUsers() {
        usersListElement.innerHTML = '';
        chatsListElement.classList.remove('d-none');
    }

    //#endregion

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