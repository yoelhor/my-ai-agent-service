// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/**
 * Retrieves an access token.
 */
async function getToken() {
    let tokenResponse;

    if (typeof getTokenPopup === 'function') {
        tokenResponse = await getTokenPopup({
            scopes: [...loginRequest.scopes],
            redirectUri: '/redirect'
        });
    } else {
        tokenResponse = await getTokenRedirect({
            scopes: [...loginRequest.scopes],
        });
    }

    if (!tokenResponse) {
        return null;
    }

    return tokenResponse.accessToken;
}

async function submitPrompt() {
    console.log("Submit button clicked");
    const accessToken = await getToken();

    if (!accessToken) {
        console.error("Unable to acquire access token.");
        return;
    }

    console.log("Access token is available and will be used for the API call");

    const userInput = document.getElementById('chatbot-input').value;
    console.log("User input:", userInput);

    // Make HTTP GET method call to backend API with the access token
    const endpoint = `/api/token`;

    try {
        const response = await fetch(endpoint, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${accessToken}`,
                'Accept': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`API request failed: ${response.status} ${response.statusText}`);
        }

        const result = await response.json();
        console.log("Backend API response:", result);

        // Add the response from the backend API to the chatbot UI
        const chatbotMessages = document.getElementById('chatbot-messages');
        const responseElement = document.createElement('div');
        responseElement.classList.add('chatbot-response-item');
        responseElement.textContent = result.message;
        chatbotMessages.appendChild(responseElement);
    } catch (error) {
        console.error("Error calling backend API:", error);
    }

}