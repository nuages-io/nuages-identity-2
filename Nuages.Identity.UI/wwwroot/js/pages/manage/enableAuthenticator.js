var App =
    {
        data() {
            return {
                code: "",
                errors: [],
                status: "",
                userName: userName
            }
        },
        mounted() {
            code.focus();
        },
        methods:
            {
                doVerifyCode: function () {
                    var self = this;
                    var c = self.code;

                    this.status = "sending";

                    fetch("/api/manage/enable2FA", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                code: c
                            }
                        )
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
                verifyCode: function () {

                    this.errors = [];
                    formCode.classList.remove("was-validated");

                    code.setCustomValidity("");

                    var res = formCode.checkValidity();
                    if (res) {
                        this.doVerifyCode();
                    } else {
                        formCode.classList.add("was-validated");

                        if (!code.validity.valid) {
                            code.setCustomValidity(codeRequiredMessage);
                        }

                        var list = formCode.querySelectorAll("input:invalid");

                        list.forEach((element) => {
                            this.errors.push({message: element.validationMessage, id: element.id});
                        });

                    }
                },
                makeCredentialOptions()
                {
                    //Call API
                    this.status = "sending";

                    var self = this;
                    var u = self.userName;

                    fetch("/api/fido2/makeCredentialOptions", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                                UserName: u,
                                DisplayName: u,
                                AttestationType: "none",
                                UserVerification: "preferred",
                                AuthType: null,
                                RequireResidentKey: false
                            }
                        )
                    })
                        .then(response => response.json())
                        .then(async res => {

                            if (res.status !== "ok")
                            {
                                res.errors.forEach((element) => {
                                    self.errors.push({message: element});
                                });
                            }
                            else
                            {
                                var makeCredentialOptions = res;
                                
                                // Turn the challenge back into the accepted format of padded base64
                                makeCredentialOptions.challenge = coerceToArrayBuffer(makeCredentialOptions.challenge);
                                // Turn ID into a UInt8Array Buffer for some reason
                                makeCredentialOptions.user.id = coerceToArrayBuffer(makeCredentialOptions.user.id);

                                makeCredentialOptions.excludeCredentials = makeCredentialOptions.excludeCredentials.map((c) => {
                                    c.id = coerceToArrayBuffer(c.id);
                                    return c;
                                });

                                if (makeCredentialOptions.authenticatorSelection.authenticatorAttachment === null) 
                                    makeCredentialOptions.authenticatorSelection.authenticatorAttachment = undefined;

                                let newCredential;
                                try {
                                    
                                    newCredential = await navigator.credentials.create({
                                        publicKey: makeCredentialOptions
                                    })

                                    await registerNewCredential(newCredential);
                                } catch (e) {
                                    var msg = "Could not create credentials in browser. Probably because the username is already registered with your authenticator. Please change username or authenticator."
                                    console.error(msg, e);
                                    showErrorAlert(msg, e);
                                }
                            }
                          
                        });

                }
            },
        watch: {
            code(value) {
                this.errors = [];
                this.status = "";
                code.setCustomValidity("");
            }
        }
    };

Vue.createApp(App).mount('#app')


coerceToArrayBuffer = function (thing, name) {
    if (typeof thing === "string") {
        // base64url to base64
        thing = thing.replace(/-/g, "+").replace(/_/g, "/");

        // base64 to Uint8Array
        var str = window.atob(thing);
        var bytes = new Uint8Array(str.length);
        for (var i = 0; i < str.length; i++) {
            bytes[i] = str.charCodeAt(i);
        }
        thing = bytes;
    }

    // Array to Uint8Array
    if (Array.isArray(thing)) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to ArrayBuffer
    if (thing instanceof Uint8Array) {
        thing = thing.buffer;
    }

    // error if none of the above worked
    if (!(thing instanceof ArrayBuffer)) {
        throw new TypeError("could not coerce '" + name + "' to ArrayBuffer");
    }

    return thing;
};


coerceToBase64Url = function (thing) {
    // Array or ArrayBuffer to Uint8Array
    if (Array.isArray(thing)) {
        thing = Uint8Array.from(thing);
    }

    if (thing instanceof ArrayBuffer) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to base64
    if (thing instanceof Uint8Array) {
        var str = "";
        var len = thing.byteLength;

        for (var i = 0; i < len; i++) {
            str += String.fromCharCode(thing[i]);
        }
        thing = window.btoa(str);
    }

    if (typeof thing !== "string") {
        throw new Error("could not coerce to string");
    }

    // base64 to base64url
    // NOTE: "=" at the end of challenge is optional, strip it off here
    thing = thing.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");

    return thing;
};

async function registerNewCredential(newCredential) {
    // Move data into Arrays incase it is super long
    let attestationObject = new Uint8Array(newCredential.response.attestationObject);
    let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);
    let rawId = new Uint8Array(newCredential.rawId);

    const data = {
        id: newCredential.id,
        rawId: coerceToBase64Url(rawId),
        type: newCredential.type,
        extensions: newCredential.getClientExtensionResults(),
        response: {
            AttestationObject: coerceToBase64Url(attestationObject),
            clientDataJson: coerceToBase64Url(clientDataJSON)
        }
    };

    let response;
    try {
        response = await registerCredentialWithServer(data);
    } catch (e) {
        showErrorAlert(e);
    }

    console.log("Credential Object", response);

    // show error
    if (response.status !== "ok") {
        console.log("Error creating credential");
        console.log(response.errorMessage);
        showErrorAlert(response.errorMessage);
        return;
    }

    // show success 
    Swal.fire({
        title: 'Registration Successful!',
        text: 'You\'ve registered successfully.',
        type: 'success',
        timer: 2000
    });

    // redirect to dashboard?
    //window.location.href = "/dashboard/" + state.user.displayName;
}

async function registerCredentialWithServer(formData) {
    let response = await fetch('/api/fido2/makeCredential', {
        method: 'POST', // or 'PUT'
        body: JSON.stringify(formData), // data can be `string` or {object}!
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });

    let data = await response.json();

    return data;
}
