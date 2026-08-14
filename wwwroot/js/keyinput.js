let dotNetRef = null;

export function initialize(dotnetReference)
{
    dotNetRef = dotnetReference
    window.addEventListener("keydown", handleKeyDown);
    window.addEventListener("keydown", handleShortcutKeys);
    window.addEventListener("keyup", handleKeyUp);
}

export function dispose() {
    window.removeEventListener("keydown", handleKeyDown);
    window.removeEventListener("keydown", handleShortcutKeys);
    window.removeEventListener("keyup", handleKeyUp);
    dotNetRef = null;
}

function handleShortcutKeys(ev)
{
    if (isTypingInInput(ev.target)) return;

    if (ev.key === '1' || ev.key === '2' || ev.key === '3') {
        ev.preventDefault();
        if (!dotNetRef) return;
        dotNetRef.invokeMethodAsync('OnShortcutKey', ev.key);
        return;
    }

    if (ev.key === 'Tab') {
        ev.preventDefault();
        if (!dotNetRef) return;
        dotNetRef.invokeMethodAsync('OnCommandShortcut', 'nextScramble');
        return;
    }

    if (ev.key === 'Delete' || ev.key === 'Backspace') {
        ev.preventDefault();
        if (!dotNetRef) return;
        dotNetRef.invokeMethodAsync('OnCommandShortcut', 'deleteCurrentSolve');
    }
}

function handleKeyDown(ev)
{
    if (!(ev.code === 'Space' || ev.key === ' ' || ev.key === 'Spacebar')) return;
    if (ev.repeat) return;  //OS autofires if you hold down a key, this exists purely to catch that
    if (isTypingInInput(ev.target)) return


    console.log("keydown", ev.code, ev.repeat) //writes to debug log
    ev.preventDefault(); //space usually causes a page to scroll, this prevents that
    dotNetRef.invokeMethodAsync("OnSpaceDown");
}

function handleKeyUp(ev)
{
   if (!(ev.code === 'Space' || ev.key === ' ' || ev.key === 'Spacebar')) return;
   if (ev.repeat) return;
   if (isTypingInInput(ev.target)) return;

   console.log("keyup", ev.code, ev.repeat)
   ev.preventDefault();
   dotNetRef.invokeMethodAsync("OnSpaceUp");
}

function isTypingInInput(target) //stops the spacebar from triggering timer when typing elsewhere (e.g. inputting solve time if using external timer)
{
    if (!target) return false;
    const tag = (target.tagName || '').toUpperCase();
    if (tag === 'INPUT' || tag === 'TEXTAREA') return true;
    if (target.isContentEditable) return true;
    return false;
}
