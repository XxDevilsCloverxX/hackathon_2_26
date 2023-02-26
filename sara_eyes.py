#import libraries
from detecto.utils import read_image
from detecto.core import Model
from detecto.visualize import show_labeled_image
import cv2
import os
import http.server
import threading
import json

class HTTPhandler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        global data_dict

        # Set response headers
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()

        # Convert dictionary to JSON format
        json_data = json.dumps(data_dict)

        # Send JSON data in response body
        self.wfile.write(json_data.encode())

#load the base directory
base = os.path.dirname(os.path.realpath(__file__))

#define server params
HOST = 'localhost'
PORT = 8080

#create a model object using the existing labels
with open(f"{base}\\labels.txt", "r") as f:
    labels = f.read().splitlines()
    f.close()

#get the names to filter from labels
with open(f"{base}\\names.txt", "r") as f:
    names = f.read().splitlines()
    f.close()

#load the model
sara_model = Model(labels)

#this method detects faces & hands in frame
def detect_people(frame, model):
    #apply the pre-trained model to frame
    labels, boxes, scores = model.predict(frame)

    #drop the predictions the model wasn't confident about
    filter = [index for index,val in enumerate(scores) if val > .35]
    boxes = boxes[filter]  #return tensors from the filter
    labels = [labels[index] for index in filter]

    #special persons cannot be a face   \_(ツ)_/¯ This doc string is for filtering special folks
    """for name in names:
        if name in labels:
            try:    
                index = labels.index("face")
                labels.remove("face")
            except:
                index = None
            finally:
                if index != None:
                    boxes = boxes[[_ for _ in range(len(boxes)) if _ != index]] #filter index of face from boxes
                #filter names from labels
                indeces = [index for index, label in enumerate(labels) if label in names and label != name]
                labels = [label for label in labels if (label not in names or label == name)]
                
                #filter names from boxes
                boxes = [box for index, box in enumerate(boxes) if index not in indeces]
            break    
    """
    #split the labels into keys & indeces
    objects = {key: index for index, key in enumerate(labels)}

    return objects, boxes

# Define a function to process a live video stream
def process_live_video(model):
    # Initialize the video capture object
    cap = cv2.VideoCapture(1)

    global data_dict

    # Loop over frames from the video stream
    while True:
        # Read the next frame from the video stream
        ret, frame = cap.read()

        # If we couldn't read the next frame, break
        if not ret:
            raise ValueError("Frame could not be read, aborting!")

        # Detect objects in a single frame
        objects, boxes = detect_people(frame, model)

        data_dict = {}
        if len(boxes) > 0:
            # Draw the bounding boxes around the detected objects
            for key, index in objects.items():
                x1, y1, x2, y2 = boxes[index]
                x1 = int(x1)
                y1 = int(y1)
                x2 = int(x2)
                y2 = int(y2)
                # Draw the bounding box
                cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 0, 255), 2)

                # Draw the label
                cv2.putText(frame, key, (x1, y1 - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 255), 1)

            #update global dictionary to send to server
            if "gunsup" in objects.keys():
                data_dict.update({"Animations": 0})
            elif "peace" in objects.keys():
                data_dict.update({"Animations": 1})
            elif "face" in objects.keys():
                data_dict.update({"Animations": 2})
            elif "wave" in objects.keys():
                data_dict.update({"Animations": 3})
            else:
                data_dict.update({"Animations": 4})

        # Display the frame
        cv2.imshow("Frame", frame)

        # Check if the user pressed the 'q' key
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break

    # Release the video capture object and close any open windows
    cap.release()
    cv2.destroyAllWindows()

if __name__ == '__main__':
    # fill architecture with the trained weights
    sara_model = Model.load(file=f"{base}/sara.pth", classes=labels)
    # Create server & start it
    httpd = http.server.ThreadingHTTPServer((HOST, PORT), HTTPhandler)
    print(f"Server started on {HOST}:{PORT}")

    #process video on another thread
    thread = threading.Thread(target=process_live_video, args=(sara_model,))

    #start the thread
    thread.start()

    #serve forever
    httpd.serve_forever()
