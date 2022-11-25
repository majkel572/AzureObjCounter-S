import datetime
import json
import sys
from azure.cognitiveservices.vision.customvision.prediction import CustomVisionPredictionClient
from msrest.authentication import ApiKeyCredentials
from PIL import Image
import requests
from io import BytesIO
from azure.storage.blob import generate_blob_sas, AccountSasPermissions

def create_token(blob_name):
    account_name='ocs654'
    container_name='objcountstrg'
    account_key='s01P1E6j1ZclpsnLscp2dypR5zpdsH+X77zAK76quIC4Jk856rnDTD9yCPuUqPDtQttAdYlU3A/z+AStj+Almw=='
    url = f"https://{account_name}.blob.core.windows.net/{container_name}/{blob_name}"
    # Creating token that allows downloading image from blob
    sas_token = generate_blob_sas(
        account_name=account_name,
        account_key=account_key,
        container_name=container_name,
        blob_name=blob_name,
        permission=AccountSasPermissions(read=True),
        expiry=datetime.datetime.utcnow() + datetime.timedelta(hours=1)
    )

    url_with_sas = f"{url}?{sas_token}"
    return url_with_sas

def main(blob_name):

    url = create_token(blob_name)

    try:
        # Get Configuration Settings
        prediction_endpoint = 'https://customvisiondetector-prediction.cognitiveservices.azure.com/'
        prediction_key = '480c10e29ebd486b90bea83ca6d082e1'
        project_id = 'aee14dc7-79af-4b9f-b42a-cfd8147dd022'
        model_name = 'Iteration1'

        # Authenticate a client for the training API
        credentials = ApiKeyCredentials(in_headers={"Prediction-key": prediction_key})
        prediction_client = CustomVisionPredictionClient(endpoint=prediction_endpoint, credentials=credentials)

        # Load image 
        response = requests.get(url)
        image = Image.open(BytesIO(response.content))
        print('Detecting objects in', image)

        # Detect objects in the test image
        with BytesIO() as buf:
            image.save(buf, 'jpeg')
            image_data = buf.getvalue()
            results = prediction_client.detect_image(project_id, model_name, image_data)

        stats = {}
        
        for prediction in results.predictions:
            # Only show objects with a > 50% probability
            if (prediction.probability*100) > 50:
                if prediction.tag_name in stats:
                    stats[prediction.tag_name] +=1
                else:
                    stats[prediction.tag_name] = 1

        json_stats = json.dumps(stats, indent = 4)
        print('Results saved in json: \n ', json_stats)
        return json_stats
    except Exception as ex:
        print(ex)

if __name__ == "__main__":
    blob_name = sys.argv[1]
    main(blob_name)