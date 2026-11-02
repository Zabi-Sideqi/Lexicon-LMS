let activeModal = null;
let previouslyFocusedElement = null;
let keydownHandler = null;

const getFocusableElements = (modal) =>
    Array.from(modal.querySelectorAll(
        'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
    )).filter(element =>
        !element.hasAttribute("hidden") &&
        element.getAttribute("aria-hidden") !== "true" &&
        element.getClientRects().length > 0
    );

export function showModal(modal) {
    if (!modal) return;

    if (activeModal && activeModal !== modal) {
        hideModal();
    }

    activeModal = modal;
    previouslyFocusedElement = document.activeElement;

    keydownHandler = (event) => {
        if (event.key !== "Tab" || !activeModal) return;

        const focusable = getFocusableElements(activeModal);

        if (focusable.length === 0) {
            event.preventDefault();
            activeModal.focus();
            return;
        }

        const first = focusable[0];
        const last = focusable[focusable.length - 1];

        if (event.shiftKey &&
            (document.activeElement === first ||
             !activeModal.contains(document.activeElement))) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey &&
                   (document.activeElement === last ||
                    !activeModal.contains(document.activeElement))) {
            event.preventDefault();
            first.focus();
        }
    };

    modal.addEventListener("keydown", keydownHandler);

    const focusable = getFocusableElements(modal);
    (focusable[0] ?? modal).focus();
}

export function hideModal() {
    if (activeModal && keydownHandler) {
        activeModal.removeEventListener("keydown", keydownHandler);
    }

    activeModal = null;
    keydownHandler = null;

    if (previouslyFocusedElement &&
        typeof previouslyFocusedElement.focus === "function" &&
        document.contains(previouslyFocusedElement)) {
        previouslyFocusedElement.focus();
    }

    previouslyFocusedElement = null;
}