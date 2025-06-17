runCaptcha = async function (actionName) {
    var googleReCaptchaToken = "";

    let execute = new Promise((resolve, reject) => {
        grecaptcha.ready(function () {
            grecaptcha.execute('6LcODbkqAAAAABY7G-X3St7_OUbdydZy6oVwsx92', { action: 'submit' }).then(function (token) {
                googleReCaptchaToken = token;
                resolve();
            });
        });
    });

    await execute;
    return googleReCaptchaToken;
};
