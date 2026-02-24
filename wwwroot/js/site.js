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

function markdownToHtml(markdown) {
    if (!markdown) {
        return '';
    }

    if (typeof marked !== 'undefined' && typeof marked.parse === 'function') {
        return marked.parse(markdown);
    }

    const escapedMarkdown = markdown
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');

    let html = escapedMarkdown
        .replace(/^### (.*$)/gim, '<h3>$1</h3>')
        .replace(/^## (.*$)/gim, '<h2>$1</h2>')
        .replace(/^# (.*$)/gim, '<h1>$1</h1>')
        .replace(/\*\*(.*?)\*\*/gim, '<strong>$1</strong>')
        .replace(/\*(.*?)\*/gim, '<em>$1</em>')
        .replace(/`([^`]+)`/gim, '<code>$1</code>')
        .replace(/\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/gim, '<a href="$2" target="_blank" rel="noopener noreferrer">$1</a>');

    html = html.replace(/(?:^|\n)(- .*(?:\n- .*)*)/g, (match) => {
        const items = match
            .trim()
            .split('\n')
            .map((item) => `<li>${item.replace(/^- /, '').trim()}</li>`)
            .join('');

        return `\n<ul>${items}</ul>`;
    });

    html = html
        .split(/\n{2,}/)
        .map((block) => {
            const trimmedBlock = block.trim();

            if (!trimmedBlock) {
                return '';
            }

            if (/^<(h1|h2|h3|ul)/i.test(trimmedBlock)) {
                return trimmedBlock;
            }

            return `<p>${trimmedBlock.replace(/\n/g, '<br>')}</p>`;
        })
        .join('');

    return html;
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

    // Make HTTP POST method call to backend API with the access token
    const endpoint = `/api/ClaudeAgent`;

    try {
        const response = await fetch(endpoint, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${accessToken}`,
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({ prompt: userInput })
        });

        if (!response.ok) {
            throw new Error(`API request failed: ${response.status} ${response.statusText}`);
        }

        const result = await response.json();
        console.log("Backend API response:", result);

        // Add the response from the backend API to the chatbot UI
        const chatbotMessages = document.getElementById('chatbot-messages');
        const responseElement = document.createElement('div');
        responseElement.classList.add('list-group-item');
        responseElement.classList.add('border-0');

        var ResponseMessage = result.message || "No response message";

        responseElement.innerHTML = markdownToHtml(ResponseMessage);

        chatbotMessages.appendChild(responseElement);


    } catch (error) {
        console.error("Error calling backend API:", error);
    }

}