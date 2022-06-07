/**
 * Clase Text field
 */
class TextField {

  constructor(editor) {
    let _self = this;
    this.editor = editor
    // const button = editor.querySelector('.editor-btn');
    const toolbar = editor.getElementsByClassName('toolbar')[0];
    const contentArea = editor.getElementsByClassName('content-area')[0];
    const visuellView = editor.getElementsByClassName('visuell-view')[0];
    const textarea = editor.getElementsByTagName('textarea')[0];

    // Oculta el textarea para sustutir la edición por el div
    textarea.classList.add('d-none')

    // Oculta el textarea para sustutir la edición por el div
    toolbar.classList.remove('d-none')
    // Oculta el textarea para sustutir la edición por el div
    visuellView.classList.remove('d-none')

    // Add class inicilized to the element
    editor.classList.add("inicilized")

    this.buttons = toolbar.querySelectorAll('.editor-btn:not(.has-submenu)');
    // button.removeEventListener('click', this.boldButton);
    // button.addEventListener('click', this.boldButton);

    // editor.removeEventListener('focusout', this.focusout)
    // editor.addEventListener('focusout', this.focusout);
    //Evita que el tab salte a otro elemento
    visuellView.removeEventListener('keydown', this.keyEvent);
    visuellView.addEventListener('keydown', this.keyEvent);

    // button.removeEventListener('mousedown', this.mouseEvent);
    // button.addEventListener('mousedown', this.mouseEvent);


    // add active tag event
    visuellView.addEventListener('selectionchange', this.selectionChange);

    // add paste event
    visuellView.removeEventListener('paste', this.pasteEvent);
    visuellView.addEventListener('paste', this.pasteEvent);

    // add paragraph tag on new line
    contentArea.addEventListener('keypress', this.keyEvent);

    // add toolbar button actions
    for(let i = 0; i < this.buttons.length; i++) {
      let button = this.buttons[i];
      
      button.addEventListener('click', function(e) {
        let action = this.dataset.action;
        _self.execDefaultAction(action);
        
      });
    }

  }

  mouseEvent(e) {
    e.preventDefault();
  }

  selectionChange(e) {
  
    for(let i = 0; i < this.buttons.length; i++) {
      let button = this.buttons[i];
      
      // don't remove active class on code toggle button
      if(button.dataset.action === 'toggle-view') continue;
      
      button.classList.remove('active');
    }
    
    if(!childOf(window.getSelection().anchorNode.parentNode, editor)) return false;
    
    this.parentTagActive(window.getSelection().anchorNode.parentNode);
  }

  parentTagActive(elem) {
    if(!elem ||!elem.classList || elem.classList.contains('visuell-view')) return false;
    
    let toolbarButton;
    
    // active by tag names
    let tagName = elem.tagName.toLowerCase();
    toolbarButton = this.editor.querySelectorAll(`.toolbar .editor-btn[data-tag-name="${tagName}"]`)[0];
    if(toolbarButton) {
      toolbarButton.classList.add('active');
    }
    
    // active by text-align
    let textAlign = elem.style.textAlign;
    toolbarButton = this.editor.querySelectorAll(`.toolbar .editor-btn[data-style="textAlign:${textAlign}"]`)[0];
    if(toolbarButton) {
      toolbarButton.classList.add('active');
    }
    
    return this.parentTagActive(elem.parentNode);
  }

  // boldButton(e) {
  //   document.execCommand("bold", false);
  //   let button = e.currentTarget;
  //   if (button.classList.contains('active')) {
  //     button.classList.remove('active');
  //   } else {
  //     button.classList.add('active');
  //   }
  // }

  /**
   * This function executes all 'normal' actions
   */
  execDefaultAction(action) {
    document.execCommand(action, false);
  }


  // focusout(e) {
  //   let button = e.target.parentNode.querySelector('.editor-btn');
  //   let texto = e.target.innerHTML;
  //   if (texto.substr(texto.length - 4) == '<br>') {
  //     e.target.lastElementChild.remove();
  //   }
  //   button.classList.remove('active');
  // }

  pasteEvent(e) {
    e.preventDefault();
    let text = (e.originalEvent || e).clipboardData.getData('text/plain');
    text = text.replaceAll("\n", "<br>");
    document.execCommand('insertHTML', false, text);
  }

  keyEvent(e) {
    if (e.keyCode == 9) {
      e.preventDefault();
      document.execCommand('insertText', false, '    ');
    } 
    //if enter, disable bold 
    else if (e.keyCode == 13) {
      let button = e.currentTarget.parentNode.querySelector('.editor-btn');
      button.classList.remove('active');
    }
  }
}
