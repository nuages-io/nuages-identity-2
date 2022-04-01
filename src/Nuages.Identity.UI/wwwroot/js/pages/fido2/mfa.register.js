
async function handleAddSecurityKey(data, callback) {
    
    // send to server for registering
    let makeCredentialOptions;
    try {
        makeCredentialOptions = await fetchMakeCredentialOptions(data);

    } catch (e) {
        console.error(e);
        let msg = "Something wen't really wrong";
        callback("error", msg);
    }

    console.log("Credential Options Object", makeCredentialOptions);

    if (makeCredentialOptions.status !== "ok") {
        console.log("Error creating credential options");
        console.log(makeCredentialOptions.errorMessage);
        callback("error", makeCredentialOptions.errorMessage);
        return;
    }

    // Turn the challenge back into the accepted format of padded base64
    makeCredentialOptions.challenge = coerceToArrayBuffer(makeCredentialOptions.challenge);
    // Turn ID into a UInt8Array Buffer for some reason
    makeCredentialOptions.user.id = coerceToArrayBuffer(makeCredentialOptions.user.id);

    makeCredentialOptions.excludeCredentials = makeCredentialOptions.excludeCredentials.map((c) => {
        c.id = coerceToArrayBuffer(c.id);
        return c;
    });

    if (makeCredentialOptions.authenticatorSelection.authenticatorAttachment === null) makeCredentialOptions.authenticatorSelection.authenticatorAttachment = undefined;

    console.log("Credential Options Formatted", makeCredentialOptions);

    callback("registering");

    console.log("Creating PublicKeyCredential...");

    let newCredential;
    try {
        newCredential = await navigator.credentials.create({
            publicKey: makeCredentialOptions
        });
    } catch (e) {
        var msg = "Could not create credentials in browser. Probably because the username is already registered with your authenticator. Please change username or authenticator."
        console.error(msg, e);
        callback("error", msg);
    }

    console.log("PublicKeyCredential Created", newCredential);

    try {
        registerNewCredential(newCredential, callback);

    } catch (e) {
        callback("error", e.message);
    }
}

async function fetchMakeCredentialOptions(formData) {
    let response = await fetch('/api/fido2/makeCredentialOptions', {
        method: 'POST', // or 'PUT'
        body: JSON.stringify(formData), // data can be `string` or {object}!
        headers: {
            'Content-Type': 'application/json',
            "X-XSRF-TOKEN": xsrfToken
        }
    });

    return await response.json();
}

// This should be used to verify the auth data with the server
async function registerNewCredential(newCredential, callback) {
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
        callback("error", e)
    }

    console.log("Credential Object", response);

    // show error
    if (response.status !== "ok") {
        console.log("Error creating credential");
        console.log(response.errorMessage);

        callback("error", response.errorMessage)
    }
    else {
        callback("success")
    }
}

async function registerCredentialWithServer(formData) {
    let response = await fetch('/api/fido2/makeCredential', {
        method: 'POST', // or 'PUT'
        body: JSON.stringify(formData), // data can be `string` or {object}!
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            "X-XSRF-TOKEN": xsrfToken
        }
    });

    return await response.json();
}