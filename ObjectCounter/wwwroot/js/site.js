function readImage(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#blah')
                .attr('src', e.target.result)
                .width(800)
                .height(500);
        };

        reader.readAsDataURL(input.files[0]);
    }
}
const delay = ms => new Promise(res => setTimeout(res, ms));

var counted;
// select file input
const input = document.getElementById('selectedFile')

// add event listener
input.addEventListener('change', () => {
  uploadFile(input.files[0])
})

const uploadFile = async file => {
    // add the file to the FormData object
    const fd = new FormData()
    fd.append('selectedFile', file)
  
    // send `POST` request
    return fetch('api/upload', {
      method: 'POST',
      body: fd
    })
    .then(response => response.text())
    .then((response) => {
        response = response.replaceAll("[{", "").replaceAll("}]",""),
        response = response.replaceAll("\"Key\":\"", "").replaceAll("\",\"\Value\"",""),
        response = response.replaceAll("},{", "\n").replaceAll(":", ": ")
        window.alert(response)
    }) 
  }