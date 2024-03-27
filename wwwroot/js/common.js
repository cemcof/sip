window.preventDefaultIfEnter = function(event) { (event.key == "Enter") ? event.preventDefault() : true; } 
window.preventDefaultIfUp = function(event) { (event.key == "ArrowUp") ? event.preventDefault() : true; }
window.preventDefaultIfDown = function(event) { (event.key == "ArrowDown") ? event.preventDefault() : true; }


// Scroll into view if needed implementation. 
// Native implementation is not supported by most of the browser - have to use this one
// From: https://gist.github.com/hsablonniere/2581101
if (!Element.prototype.scrollIntoViewIfNeeded) {
  Element.prototype.scrollIntoViewIfNeeded = function (centerIfNeeded) {
    centerIfNeeded = arguments.length === 0 ? true : !!centerIfNeeded;

    var parent = this.parentNode,
        parentComputedStyle = window.getComputedStyle(parent, null),
        parentBorderTopWidth = parseInt(parentComputedStyle.getPropertyValue('border-top-width')),
        parentBorderLeftWidth = parseInt(parentComputedStyle.getPropertyValue('border-left-width')),
        overTop = this.offsetTop - parent.offsetTop < parent.scrollTop,
        overBottom = (this.offsetTop - parent.offsetTop + this.clientHeight - parentBorderTopWidth) > (parent.scrollTop + parent.clientHeight),
        overLeft = this.offsetLeft - parent.offsetLeft < parent.scrollLeft,
        overRight = (this.offsetLeft - parent.offsetLeft + this.clientWidth - parentBorderLeftWidth) > (parent.scrollLeft + parent.clientWidth),
        alignWithTop = overTop && !overBottom;

    if ((overTop || overBottom) && centerIfNeeded) {
      parent.scrollTop = this.offsetTop - parent.offsetTop - parent.clientHeight / 2 - parentBorderTopWidth + this.clientHeight / 2;
    }

    if ((overLeft || overRight) && centerIfNeeded) {
      parent.scrollLeft = this.offsetLeft - parent.offsetLeft - parent.clientWidth / 2 - parentBorderLeftWidth + this.clientWidth / 2;
    }

    if ((overTop || overBottom || overLeft || overRight) && !centerIfNeeded) {
      this.scrollIntoView(alignWithTop);
    }
  };
}  

// For blazor interop - need to have this on window object and accept element 
window.scrollIntoViewIfNeeded = function(element) {
    element.scrollIntoViewIfNeeded()
}

// For copying to clipboard
window.copyToClipboard = function(text) {
  
    console.log("Copying to clipboard: " + text);
    
    navigator.clipboard.writeText(text)
        .then(() => {
            console.log('Text copied to clipboard');
        }, (err) => {
            console.error('Error copying text to clipboard: ', err);
        });
}

window.copyFromElementToClipboard = function(element) {
  window.copyToClipboard(element.innerText);
}

window.copyFromSiblingElementToClipboard = function(element) {
    window.copyToClipboard(element.nextElementSibling.innerText);
}