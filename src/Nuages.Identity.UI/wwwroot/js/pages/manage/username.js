var App =
    {
        data() {
            return {
                username: "",
                currentUserName: currentUserName,
                errors: [],
                status: ""
            }
        },
        mounted() {
            username.focus();
        },
        methods:
            {
                doChangeUsername: function () {
                    var self = this;
                    var u = self.username;

                    this.status = "sending";

                    fetch("/app/manage/changeUsername", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        },
                        body: JSON.stringify({
                                newUsername: u
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            if (res.success) {

                                self.currentUserName = self.username;

                                updateUserName(self.currentUserName);

                                self.username = null;
                                self.status = "done";

                                setTimeout(function () {
                                    username.focus();
                                })
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({message: element});
                            });
                        });

                },
                changeUsername: function () {

                    this.errors = [];
                    formChangeUsername.classList.remove("was-validated");

                    username.setCustomValidity("");

                    var res = formChangeUsername.checkValidity();
                    if (res) {
                        this.doChangeUsername();
                    } else {
                        formChangeUsername.classList.add("was-validated");

                        if (!username.validity.valid) {
                            username.setCustomValidity(usernameRequiredMessage);
                        }

                        var list = formChangeUsername.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });
                    }
                }
            },
        watch: {
            username(value) {
                this.errors = [];
                if (value != null)
                    this.status = "";

                username.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')