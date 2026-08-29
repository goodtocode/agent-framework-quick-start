export function scrollToBottomById(elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        return;
    }

    requestAnimationFrame(() => {
        element.scrollTop = element.scrollHeight;
    });
}
