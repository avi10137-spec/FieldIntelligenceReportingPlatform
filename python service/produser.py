
from confluent_kafka import Producer
import json

config = {"bootstrap.servers": "broker:9092"}
producer = Producer(config)
topic_name = "raw-data-first"
file_path = "field_reports.json"
def delivery_report(err, msg):
    if err is not None:
        print(f"Message delivery failed: {err}")
    else:
        print("produce is succseesfuli")
try:
    with open(file_path, 'r', encoding='utf-8') as file:
        data = json.load(file)
        for row in data:
            newline = json.dumps(row)
            producer.produce(topic_name,value=newline,callback=delivery_report)
            producer.poll(0)
        producer.flush()
        print("Finished sending all messages.")
except FileNotFoundError:
    print(f"Error: The file '{file_path}' was not found.")
except Exception as e:
    print(f"An unexpected error occurred: {e}")