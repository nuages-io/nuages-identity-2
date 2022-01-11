var App =
    {
        data(){
            return {
                errors: [],
                status : ""
            }
        },
        methods:
            {
                send : function()
                {
                    this.status = "sending";
                    
                    fetch("/api/account/sendEmailConfirmation",
                        {
                            method: "POST",
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                                  
                                }
                            )
                        }).then(response => response.json())
                        .then(res => {
                            this.status = "sent";
                        });                    
                }
            }
    };

Vue.createApp(App).mount('#app')