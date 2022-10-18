FROM ubuntu:20.04

RUN apt update && DEBIAN_FRONTEND=noninteractive apt -y install python3 python3-pip apache2 libapache2-mod-wsgi-py3 git wget poppler-utils

WORKDIR /hercules-enrichment-api

COPY requirements.txt .
RUN pip install -r requirements.txt
RUN spacy download en_core_web_lg
RUN spacy download es_core_news_lg

RUN wget https://storage.googleapis.com/elhuyar/Hercules/hercules-models.tar.gz
RUN tar xfz hercules-models.tar.gz

COPY . .

COPY docker_resources/enrichment_api.conf /etc/apache2/sites-available/
COPY docker_resources/enrichment_api.wsgi /hercules-enrichment-api/
COPY docker_resources/conf.json /hercules-enrichment-api/
RUN mkdir /var/log/hercules_enrichment

RUN a2ensite enrichment_api.conf

RUN mkdir /var/www/.cache arxiv-data
RUN chgrp www-data /var/www/.cache arxiv-data
RUN chmod g+w /var/www/.cache arxiv-data

EXPOSE 80
CMD ["apachectl", "-D", "FOREGROUND"]
