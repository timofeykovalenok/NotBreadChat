window.addEventListener('DOMContentLoaded', () => {
    document.addEventListener('submit', onFormSubmit);
    document.addEventListener('input', onInput);

    async function onFormSubmit(e: SubmitEvent) {
        e.preventDefault();

        let form = e.target as HTMLFormElement;
        if (!form.checkValidity())
            return;

        let response = await fetch(form.action, {
            method: 'POST',
            body: new FormData(form)
        });

        if (response.redirected) {
            window.location.assign(response.url);
            return;
        }

        if (response.status < 400)
            return;

        let invalidFields = form.querySelectorAll(`.invalid-${response.status}`);
        invalidFields.forEach(field => {
            field.classList.add('is-invalid');
        });

        let validationFeedback = form.querySelector('.invalid-feedback');
        validationFeedback.classList.remove('d-none');
        validationFeedback.textContent = await response.text();
    }

    function onInput(e: InputEvent) {
        let target = e.target as HTMLElement;

        if (!target.classList.contains('is-invalid'))
            return;

        let form = target.closest('form');

        let invalidFields = form.querySelectorAll('.is-invalid');
        for (let field of invalidFields) {
            field.classList.remove('is-invalid');
        }        

        let validationFeedback = form.querySelector('.invalid-feedback');
        validationFeedback.classList.add('d-none');
    }
});