var App =
    {
        data() {
            return {
                showDeactivate: false,
                showRemovePhone: false,
                showCodes: false,
                recoveryCodesString: recoveryCodesString,
                recoveryCodes: recoveryCodes,
                isRemembered: isRemembered,
                userName: userName,
                errors: [],
                status: ""
            }
        },
        mounted() {

        },
        methods:
            {
                copy() {
                    navigator.clipboard.writeText(this.recoveryCodesString).then(function () {
                        /* clipboard successfully set */
                    }, function () {
                        /* clipboard write failed */
                    });
                },
                resetCodes() {
                    var self = this;

                    fetch("/api/manage/resetRecoveryCodes", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        }
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {

                                self.recoveryCodes = res.codes;
                                self.showCodes = true;
                            } else {

                            }
                        });
                },
                forgetBrowser() {
                    var self = this;

                    fetch("/api/manage/forgetBrowser", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        }
                    })
                        .then(response => response.json())
                        .then(res => {
                            self.isRemembered = false;
                        });
                },
                disable2Fa() {
                    //Call API
                    this.status = "sending";

                    fetch("/api/manage/disable2FA", {
                        method: "DELETE",
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        }
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                window.location = "/account/manage/TwoFactorAuthentication";
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({message: element});
                            });
                        });
                },
                removePhone() {
                    //Call API
                    this.status = "sending";

                    fetch("/api/manage/removePhone", {
                        method: "DELETE",
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        }
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                window.location = "/account/manage/TwoFactorAuthentication";
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({message: element});
                            });
                        });
                },
                removeKey(id) {
                    //Call API
                    this.status = "sending";

                    fetch("/api/fido2/removeKey", {
                        method: "DELETE",
                        body: JSON.stringify({ id: id}),
                        headers: {
                            'Content-Type': 'application/json',
                            "X-XSRF-TOKEN": xsrfToken
                        }
                    })
                        .then(response => response.json())
                        .then(res => {
                            window.location.reload();
                        });
                }
                
            }
    };


Vue.createApp(App).mount('#app')