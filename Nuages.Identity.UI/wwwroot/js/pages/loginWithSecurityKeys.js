var App =
    {
        data() {
            return {
                code: "",
                errors: [],
                remember: rememberMe,
                rememberMachine: false,
                userName: userName,
                action: "",
                status: ""
            }
        },
        mounted: function () {
            this.login()
        },
        methods:
            {                
                login: function () {

                    var self = this;
                    var u = self.userName;
                    
                    handleSignInSubmit({
                        UserName: u
                    }, this.callback);                    
                },
                retry: function()
                {
                  window.location.reload();  
                },
                callback: function (state, data)
                {
                    switch(state)
                    {
                        case "error":
                        {
                            this.status = "";
                            this.errors.push({message: data});
                            break;
                        }
                        case "waiting":
                        {
                            this.status = "waiting";
                            break;
                        }
                        case "done":
                        {
                            this.status = "done";
                           
                            break;
                        }
                    }
                }                
            },
        watch: {
        }
    };

Vue.createApp(App).mount('#app')

