var App =
    {
        data() {
            return {
                lastName: lastNameValue,
                firstName: firstNameValue,
                errors: [],
                status: ""
            }
        },
        mounted() {
            lastName.focus();
        },
        methods:
            {
                doSaveProfile: function () {
                    var self = this;

                    var l = self.lastName;
                    var f = self.firstName;

                    fetch("/app/manage/saveProfile", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        },
                        body: JSON.stringify({
                                lastName: l,
                                firstName: f
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(res => {

                            if (res.success) {
                                self.status = "done";
                                self.errors = [];

                                setTimeout(function () {
                                    lastName.focus();
                                })
                            } else {
                                self.status = "error";
                                self.errors = res.errors.map(function (m) {
                                    return {message: m}
                                });
                            }
                        });
                },
                saveProfile: function () {

                    var self = this;

                    this.errors = [];
                    formProfile.classList.remove("was-validated");

                    var res = formProfile.checkValidity();
                    if (res) {
                        this.status = "sending";
                        self.doSaveProfile();
                    } else {

                        formProfile.classList.add("was-validated");

                        var list = formProfile.querySelectorAll("input:invalid");

                        list.forEach((element) => {

                            this.errors.push({message: element.validationMessage, id: element.id});
                        });
                    }
                }
            },
        watch: {

            lastName(value) {
                this.errors = this.errors.filter(a => a.id !== "lastName");
                if (value != null)
                    this.status = "";
                lastName.setCustomValidity("");
            },
            firstName(value) {

                this.errors = this.errors.filter(a => a.id !== "firstName");
                if (value != null)
                    this.status = "";
                firstName.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')