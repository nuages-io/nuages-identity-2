var App =
    {
        data() {
            return {
                email: "",
                errors: [],
                status: ""
            }
        },
        mounted() {
            email.focus();
        },
        methods:
            {
                doChangeEmail: function () {
                    var self = this;
                    var e = self.email;

                    this.status = "sending";

                    fetch("/app/manage/sendEmailChange", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        },
                        body: JSON.stringify({
                                email: e
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                self.email = null;
                                self.status = "done";
                                setTimeout(function () {
                                    email.focus();
                                });
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                this.errors.push({message: element});
                            });
                        });
                },
                changeEmail: function () {
                    this.errors = [];
                    formChangeEmail.classList.remove("was-validated");

                    email.setCustomValidity("");

                    var res = formChangeEmail.checkValidity();
                    if (res) {
                        this.doChangeEmail();
                    } else {
                        formChangeEmail.classList.add("was-validated");

                        if (!email.validity.valid) {
                            if (email.validity.valueMissing) {
                                email.setCustomValidity(emailRequiredMessage);
                            } else {
                                email.setCustomValidity(emailInvalidMessage);
                            }
                        }

                        var list = formChangeEmail.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });
                    }
                }
            },
        watch: {
            email(value) {
                this.errors = [];

                if (value != null)
                    this.status = "";

                email.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')