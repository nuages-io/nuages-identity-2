var App =
    {
        data() {
            return {                
                showDeactivate: false,
                showRemovePhone: false,
                showCodes: false,
                recoveryCodes: recoveryCodes,
                errors: [],
                status: ""
            }
        },
        mounted() {
            
        },
        methods:
            {
                copy ()
                {
                    // var myTemporaryInputElement = document.createElement("input");
                    // myTemporaryInputElement.type = "text";
                    // myTemporaryInputElement.value = this.recoveryCodes;
                    //
                    // document.body.appendChild(myTemporaryInputElement);
                    //
                    // myTemporaryInputElement.select();
                    // document.execCommand("Copy");
                    //
                    // document.body.removeChild(myTemporaryInputElement);

                    navigator.clipboard.writeText(this.recoveryCodes).then(function() {
                        /* clipboard successfully set */
                    }, function() {
                        /* clipboard write failed */
                    });
                },
                resetCodes()
                {
                    
                }, 
                forgetBrowser()
                {
                    //Call API
                },
                disable2Fa()
                {
                    //Call API
                    this.status = "sending";

                    fetch("/api/manage/disable2FA", {
                        method: "DELETE",
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                window.location = "/account/manage/TwoFactorAuthentication";
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });
                        });
                },
                removePhone()
                {
                    //Call API
                    this.status = "sending";

                    fetch("/api/manage/removePhone", {
                        method: "DELETE",
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    })
                        .then(response => response.json())
                        .then(res => {
                            if (res.success) {
                                window.location = "/account/manage/TwoFactorAuthentication";
                            } else
                                self.status = "";

                            res.errors.forEach((element) => {
                                self.errors.push({ message : element});
                            });
                        });
                }               
                
            }
    };


Vue.createApp(App).mount('#app')