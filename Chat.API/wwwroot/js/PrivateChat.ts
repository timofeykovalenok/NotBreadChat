globalThis.privateChatPage = class PrivateChatPage implements Page {

    //#region Fields

    private observer: IntersectionObserver;
    private editingMessageId: number;

    private readonly currentUserId = parseInt(document.getElementById('current-user-id').getAttribute('value'));
    private readonly otherUserId = parseInt(window.location.pathname.split('/').at(-1));

    private readonly messagesList = document.getElementById('messages-list');

    private readonly writeMessageButton = document.getElementById('write-message-button') as HTMLButtonElement;
    private readonly writeMessageField = document.getElementById('write-message-field') as HTMLTextAreaElement;

    private readonly editingMessageBlock = document.getElementById('editing-message-block');
    private readonly editingMessageOldContent = this.editingMessageBlock.querySelector('.old-message-content');
    private readonly editMessageConfirmButton = document.getElementById('edit-message-confirm') as HTMLButtonElement;
    private readonly editMessageCancelButton = document.getElementById('edit-message-cancel') as HTMLButtonElement;

    private readonly scrollDownButton = document.getElementById('scroll-down-button') as HTMLButtonElement;
    private readonly unviewedMessagesCounter = this.scrollDownButton.querySelector('.unviewed-messages-count[data-count]');

    //#endregion

    //#region Public Methods

    initialize() {
        formatDatesLocally(this.messagesList);

        this.observer = new IntersectionObserver(this.onMessageIntersection, { threshold: 0.5 });
        const unviewedMessages = this.messagesList.querySelectorAll('.message[data-unviewed]');
        unviewedMessages.forEach(message => {
            this.observer.observe(message);
        });

        this.messagesList.addEventListener('scroll', this.onMessagesListScroll);
        this.messagesList.addEventListener('scrollend', this.onMessagesListScrollEnd);
        this.messagesList.addEventListener('click', this.onMessagesListClick);
        this.writeMessageButton.addEventListener('click', this.onWriteMessageClick);
        this.writeMessageField.addEventListener('keydown', this.onWriteMessageFieldKeyDown);
        this.editMessageConfirmButton.addEventListener('click', this.onEditMessageConfirmClick);
        this.editMessageCancelButton.addEventListener('click', this.onEditMessageCancelClick);
        this.scrollDownButton.addEventListener('click', this.onScrollDownButtonClick);        

        this.restoreSavedScrollValue();

        hubConnection.on('newMessage', this.onNewMessageEvent);
        hubConnection.on('messageViewed', this.onMessageViewedEvent);
        hubConnection.on('messageEdited', this.onMessageEditedEvent);
        hubConnection.on('messageDeleted', this.onMessageDeletedEvent);
    }

    dispose() {
        this.observer?.disconnect();

        hubConnection.off('newMessage', this.onNewMessageEvent);
        hubConnection.off('messageViewed', this.onMessageViewedEvent);
        hubConnection.off('messageEdited', this.onMessageEditedEvent);
        hubConnection.off('messageDeleted', this.onMessageDeletedEvent);
    }

    //#endregion

    //#region DOM Events    

    private onMessageIntersection = (entries: IntersectionObserverEntry[], observer: IntersectionObserver) => {
        let lastEntry = entries.findLast(entry => entry.isIntersecting);

        if (lastEntry == null)
            return;

        observer.unobserve(lastEntry.target);
        hubConnection.send('viewMessage', {
            messageId: parseInt(lastEntry.target.getAttribute('data-message-id'))
        });
    }      

    private onMessagesListScroll = () => {
        if (this.IsScrolledToEnd())
            this.scrollDownButton.classList.add('d-none');
        else
            this.scrollDownButton.classList.remove('d-none');
    }

    private onMessagesListScrollEnd = () => {
        let scrollStorageKey = `scroll-chat-value-${this.currentUserId}-${this.otherUserId}`;
        if (this.IsScrolledToEnd())
            window.localStorage.removeItem(scrollStorageKey);
        else
            window.localStorage.setItem(scrollStorageKey, this.messagesList.scrollTop.toString());
    }

    private onMessagesListClick = (e: MouseEvent) => {
        let targetElement = e.target as Element;
        let messageAction = targetElement?.getAttribute('data-message-action');

        if (messageAction == null)
            return;

        let messageElement = targetElement.closest('.message[data-message-id]') as HTMLElement;

        switch (messageAction) {
            case 'edit':
                this.startMessageEditing(messageElement);
                break;
            case 'delete-locally':
                hubConnection.send('deleteMessageLocally', {
                    messageId: parseInt(messageElement.getAttribute('data-message-id'))
                });
                break;
            case 'delete':
                hubConnection.send('deleteMessage', {
                    messageId: parseInt(messageElement.getAttribute('data-message-id'))
                });
                break;
            default:
                return;
        }
    }    

    private onWriteMessageClick = async () => {
        this.writeMessage();
    }

    private onWriteMessageFieldKeyDown = (e: KeyboardEvent) => {
        if (e.ctrlKey && e.key == 'Enter')
            this.writeMessage();
    }

    private onEditMessageConfirmClick = () => {
        let newMessageContent = this.writeMessageField.value.trim();
        if (newMessageContent == '')
            return;

        hubConnection.send('editMessage', { messageId: this.editingMessageId, content: newMessageContent });

        this.endMessageEditing();
    }

    private onEditMessageCancelClick = () => {
        this.endMessageEditing();
    }

    private onScrollDownButtonClick = () => {
        this.scrollToEnd();
    }

    //#endregion

    //#region Hub Events

    private onNewMessageEvent = (data) => {
        let fromCurrentChat = (data.message.model.authorId == this.otherUserId && data.message.model.receiverUserId == this.currentUserId)
            || (data.message.model.receiverUserId == this.otherUserId && data.message.model.authorId == this.currentUserId)
        if (!fromCurrentChat)
            return;

        let scrolledToEnd = this.IsScrolledToEnd();

        this.messagesList.insertAdjacentHTML('beforeend', data.message.html);

        let newMessage = this.messagesList.lastElementChild.querySelector('.message');
        formatDatesLocally(newMessage);

        if (newMessage.hasAttribute('data-unviewed')) {
            this.observer.observe(newMessage);

            let unviewedMessagesCount = parseInt(this.unviewedMessagesCounter.getAttribute('data-count'));
            this.updateUnviewedMessagesCounters(unviewedMessagesCount + 1);
        }

        if (scrolledToEnd)
            this.scrollToEnd();
    }

    private onMessageViewedEvent = (model) => {
        if (model.viewedByUserId == this.currentUserId) {

            this.updateUnviewedMessagesCounters(model.unviewedMessagesLeft);
            return;
        }
    }

    private onMessageEditedEvent = (model) => {
        let editedMessage = this.messagesList.querySelector(`.message[data-message-id="${model.messageId}"]`);
        if (editedMessage == null)
            return;

        editedMessage.querySelector('.message-content').textContent = model.content;
        editedMessage.classList.add('message-edited');
    }

    private onMessageDeletedEvent = (model) => {
        let deletedMessage = this.messagesList.querySelector(`.message[data-message-id="${model.messageId}"]`);
        deletedMessage?.remove();
    }

    //#endregion

    //#region Private Methods

    private restoreSavedScrollValue() {
        let savedScrollValue = window.localStorage.getItem(`scroll-chat-value-${this.currentUserId}-${this.otherUserId}`);
        if (savedScrollValue != null) {
            this.messagesList.scroll(0, parseInt(savedScrollValue));
            return;
        }

        let firstUnviewedMessage = this.messagesList.querySelector('.message[data-unviewed]');
        if (firstUnviewedMessage != null)
            firstUnviewedMessage.scrollIntoView();
        else
            this.messagesList.scroll(0, Number.MAX_SAFE_INTEGER);
    }

    private updateUnviewedMessagesCounters(value: number) {
        this.unviewedMessagesCounter.setAttribute('data-count', value.toString());
    } 

    private async writeMessage() {
        this.writeMessageField.focus();

        let messageContent = this.writeMessageField.value.trim();
        if (messageContent == '')
            return;

        this.writeMessageField.value = '';
        const otherUserId = parseInt(window.location.pathname.split('/').at(-1));

        await hubConnection.invoke('sendPrivateChatMessage', { receiverId: otherUserId, content: messageContent });

        this.scrollToEnd();
    }

    private IsScrolledToEnd(): Boolean {
        return this.messagesList.scrollHeight - (this.messagesList.scrollTop + this.messagesList.clientHeight) <= 30;
    }

    private scrollToEnd() {
        this.messagesList.scrollTo(0, this.messagesList.scrollHeight);
    }

    private startMessageEditing(messageElement: HTMLElement) {
        this.writeMessageField.focus();

        this.editingMessageId = parseInt(messageElement.getAttribute('data-message-id'));

        this.writeMessageButton.classList.add('d-none');
        this.editMessageConfirmButton.classList.remove('d-none');
        this.editingMessageBlock.classList.remove('d-none');

        let oldMessageContent = messageElement.querySelector('.message-content').textContent;
        this.writeMessageField.value = oldMessageContent;
        this.editingMessageOldContent.textContent = oldMessageContent;
    }

    private endMessageEditing() {
        this.writeMessageField.focus();

        this.editingMessageId = null;

        this.writeMessageButton.classList.remove('d-none');
        this.editMessageConfirmButton.classList.add('d-none');
        this.editingMessageBlock.classList.add('d-none');

        this.writeMessageField.value = '';
    }

    //#endregion
}