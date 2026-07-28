let dotNetRef = null;

export function initialize(dotnetReference)
{
    dotNetRef = dotnetReference
    window.addEventListener("keydown", handleKeyDown) 
    window.addEventListener("keydown", (ev) => {console.log(ev.key)});  //keyboard event(ev) -> logs in debug, handles keypress   
    window.addEventListener("keyup", handleKeyUp);
    window.addEventListener("keyup", (ev) => {console.log(ev.key)} )  //keyboard event (ev) -> log in depug, handles key release
}

export function disposeInput(ev) //removes everything after initializer, if not the keypress continues firing forever and calling whatever function is tied to it -> prevent memory leaks etc. -> better ux
{
    window.removeEventListener("keydown", handleKeyDown);
    window.removeEventListener("keyup", handleKeyUp);
    dotNetRef = null;
}

function handleKeyDown(ev)
{
    if (ev.code !== 'Space') return;
    if (ev.repeat) return;  //OS autofires if you hold down a key, this exists purely to catch that
    if (isTypingInInput(ev.target)) return


    console.log("keydown", ev.code, ev.repeat) //writes to debug log
    ev.preventDefult(); //space usually causes a page to scroll, this prevents that
    dotNetRef.invokeMethodAsync("OnSpaceDown");
}

function handleKeyUp(ev)
{
   if (ev.code !== 'Space') return;
   if (ev.repeat) return;
   if (isTypingInInput(ev.target)) return;

   console.log("keyup", ev.code, ev.repeat)
   ev.preventDefult();
   dotNetRef.invokeMethodAsync("OnSpaceUp");
}

function isTypingInInput(target)
{
    const tag = target.tagName;
    return tag ==
}
