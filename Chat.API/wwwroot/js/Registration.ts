//document.getElementById('registration-form').addEventListener('submit', async e => {
//    e.preventDefault();

//    let formData = new FormData(e.target as HTMLFormElement);

//    let headers = new Headers();
//    headers.append('Content-Type', 'application/json');
//    let response = await fetch('/Identity/Register', {
//        headers: headers,
//        method: 'POST',
//        body: JSON.stringify(Object.fromEntries(formData))
//    });
//    console.log(response);
//});