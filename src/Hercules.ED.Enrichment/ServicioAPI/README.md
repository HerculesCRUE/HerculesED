# Description

API providing access for the extraction of thematic and specific descriptors from scientific papers, protocols and code projects.

This API supports english and spanish languages. No parameters must be set to indicate the language of a given RO, as it is automatically identified when needed.

# Requirements

These are the minimum hardware and software requirements to run this service:
```
- 32 GB RAM memory (24 GB might be enough, but having 32 GB is recommended)
- Python 3.7
```

# Setting up with Docker

The API is wrapped up in a single container. The container holds the dependencies and the models to perform the enrichment. It configures the service to run under Apache.

To create and run the docker image you can use docker compose:
```
$ docker compose up
```

The statements to build the docker image are located in Dockerfile,
and the definition of the docker service in docker-compose.yaml.

By default, the service is mapped to the port 5080 on the host system.

# Installation and deployment

Install Ubuntu dependencies:
```
$ sudo apt install python3-dev poppler-utils
```

Create a vritual environment with the necessary requirements:
```
$ virtualenv -p python3 venv
$ source venv/bin/activate
$ pip install -r requirements.txt
```

Download the Spacy model:
```
$ spacy download en_core_web_lg
$ spacy download es_core_news_lg
```

Download the pretrained models and extract them:

```
wget https://storage.googleapis.com/elhuyar/Hercules/hercules-models.tar.gz
tar xfz hercules-models.tar.gz
```

Create a copy of `conf.template.json` with the name `conf.json`, and change the values as needed:
- Set your classifiers paths (paths of the models extracted from `hercules-models.tar.gz`)
- Set your security credentials (not required for the demo)

Deploy the API either within an standalone server or behind apache
- Standalone server: Run server (on port 8976 in the example. It takes 5-10 minutes to load the models.)
```  
$ ./run.sh 8976
```

- Behind Apache: be sure you have mod_wsgi (python3) installed
Copy the provided apache configuration into /etc/apache2/sites-available:
```
$ sudo cp gnoss_hercules_api.conf /etc/apache2/sites-available
```
Change the ```installation_path``` variable in app.wsgi file according to your needs (default value is (```/mnt/ebs/gnoss_hercules_api```)

Activate the site configuration and restart Apache
```
$sudo a2ensite gnoss_hercules_api.conf
$sudo service apache2 reload
```

NOTE: not tested with other servers

	
# User guide

La API tiene dos ENDPOINTs, uno para categorías temáticas y otro para la extracción de términos específicos. A continuación se detalla el funcionamiento de cada uno de ellos:


## ENDPOINT Categorías temáticas

La API devuelve las etiquetas clasificadas para un clasificador elegido (disponibles: sourceForge=Código, bio-protocol=protocolos, papers=artículos científicos). El clasificador se especifica mediante el parámetro "rotype" en el json de entrada. 


## Process requests

URL: https://herculesapi.elhuyar.eus/thematic

Método HTTP: POST

```
curl -X POST -H 'Content-Type: application/json' -i 'http://herculesapi.elhuyar.eus/thematic' --data '{"rotype":"...", ...}'
```

Datos de entrada (JSON): 
```
{
	"rotype": "tipo de clasificador a utilizar. Valores posibles: 'sourceForge'=Código, 'bio-protocol'=protocolos, 'papers'=artículos científicos",
	"pdf_url":"URL de la versión PDF del artículo.",
	"title": "Titulo del paper",
	"abstract": "Abstract del paper",
	"journal": "Nombre de la publicación",
	"author_name":"Nombre de l@s autor(es)",
	"author_affiliation":"Afiliación de l@s autor(es)"
}
```

IMPORTATE:
- El parametro 'rotype' es obligatorio. 
- Si se especifica el parámetro 'pdf_url' el sistema intentará extraer el texto a partir de la URL  proporcionada. Si eso no es posible, se utilizan el resto de campos porporcionados para obetener el texto a clasificar. El sistema devuelve un error si ninguno de los campos está disponible.


**Papers**

Ejemplo CURL:

- Solo con pdf_url:
```
curl -X POST -H 'Content-Type: application/json' -i 'http://herculesapi.elhuyar.eus/thematic' --data '{
     "pdf_url":"https://arxiv.org/pdf/1808.06394",
     "rotype":"papers"
}'
```
Respuesta:
```
{
   "pdf_url" : "https://arxiv.org/pdf/1808.06394",
   "rotype" : "papers",
   "topics" : [
      {
         "porcentaje" : "1.00",
         "word" : "Artificial Intelligence"
      },
      {
         "porcentaje" : "0.82",
         "word" : "Statistics and Probability"
      }
   ]
}
```

- Sin pdf_url:
```
curl -X POST -H 'Content-Type: application/json' -i 'http://herculesapi.elhuyar.eus/thematic' --data '{
    "title": "Faster Support Vector Machines",
    "abstract": "The time complexity of support vector machines (SVMs) prohibits training on huge data sets with millions of data points. Recently, multilevel approaches to train SVMs have been developed to allow for time-efficient training on huge data sets. While regular SVMs perform the entire training in one -- time consuming -- optimization step, multilevel SVMs first build a hierarchy of problems decreasing in size that resemble the original problem and then train an SVM model for each hierarchy level, benefiting from the solved models of previous levels. We present a faster multilevel support vector machine that uses a label propagation algorithm to construct the problem hierarchy. Extensive experiments indicate that our approach is up to orders of magnitude faster than the previous fastest algorithm while having comparable classification quality. For example, already one of our sequential solvers is on average a factor 15 faster than the parallel ThunderSVM algorithm, while having similar classification quality.",
    "author_affiliation": "Institute for Theoretical Informatics, Karlsruhe Institute of Technology, Karlsruhe, Germany. University of Vienna, Faculty of Computer Science, W ahringer Str. 29, 1080 Vienna, Austria.",
    "author_name": "Sebastian Schlag & Matthias Schmitt & Christian Schulz",
    "rotype": "papers"
}'
```

Repuesta:
```
{
   ...
   "title" : "Faster Support Vector Machines",
   "topics" : [
      {
         "porcentaje" : "0.90",
         "word" : "Artificial Intelligence"
      },
      {
         "porcentaje" : "0.60",
         "word" : "Distributed, Parallel, and Cluster Computing"
      }
   ]
}
```


**BioProtocol**

Ejemplo CURL:
```
$ curl -X POST -H 'Content-Type: application/json' -i 'http://herculesapi.elhuyar.eus/thematic' --data '{
  "title":"Nitrate Assay for Plant Tissues", 
  "abstract":"Nitrogen is an essential macronutrient for plant growth and nitrate content in plants can reflect the nitrogen supply of soil. Here, we provide the salicylic acid method to evaluate the nitrate content in plant tissues. The method is reliable and stable, thus it can be a good choice for measurement of nitrate in plant tissues. Nitrogen is an important macronutrient required by plants for normal growth and development. Usually most plants absorb nitrogen mainly in the form of nitrate grown under aerobic conditions (Xu et al., 2016). To determine the nitrate accumulation in plants, we need to test the nitrate content in different tissues of plants. There are some methods for determination of nitrate, for example, potentiometric method (Carlson and Keeney, 1971), phenoldisulfonic acid method (Bremner, 1965), Cadium reduction (Huffman and Barbarick, 1981) and other methods. These methods have some disadvantages, such as lower sensitivity, interferences, technician exposure to carcinogenic chemicals (Cataldo et al., 1975; Vendrell and Zupancic, 1990)Here, we provide the salicylic acid method that is free of interferences, reliable and stable. Nitrosalicylic acid is formed by the reaction of nitrate and salicylic acid under highly acidic conditions. The complex is yellow under basic (pH > 12) condition with maximal absorption at 410 nm. The absorbance is directly proportional to nitrate content. Therefore the nitrate content in tissues can be calculated based on their absorbances. This method is suitable for determination of nitrate concentration in plants.1.5 ml Eppendorf tubes 12-ml plastic culture tube (Greiner Bio One, catalog number: 184261 ) Quartz cuvettes Arabidopsis thaliana roots and/or shoots (7-day-old seedlings) Potassium nitrate (KNO3) (Sinopharm Chemical Reagent, catalog number: 10017218 ) Deionized water MS medium Liquid nitrogen Salicylic acid (Sinopharm Chemical Reagent, catalog number: 30163517 ) Sulphuric acid (98%) (Sinopharm Chemical Reagent, catalog number: 100216008 ) Sodium hydroxide (NaOH) (Sinopharm Chemical Reagent, catalog number: 10019718 ) 500 mg/L (0.0357 mol/L) KNO3 standard solution (see Recipes) 5% (w/v) salicylic acid-sulphuric acid (see Recipes) 8% (w/v) NaOH solution (see Recipes)Excel Standard curveTo make the standard curve, 1 ml, 2 ml, 3 ml, 4 ml, 6 ml, 8 ml, 10 ml, and 12 ml NO3- standard solution (500 mg/L) is transferred to eight 50 ml flasks respectively, and deionized water is added to each solution to bring the total volume to 50 ml. The concentration of the series of standard solution should be 10, 20, 30, 40, 60, 80, 100, and 120 mg/L, respectively. And the molarity of 10, 20, 30, 40, 60, 80, 100, and 120 mg/L KNO3 is 0.0007, 0.0014, 0.0021, 0.0029, 0.0043, 0.0057, 0.0071, 0.0086 mol/L, respectively.Transfer 0.1 ml of each standard solution into a 12-ml tube, respectively. Use 0.1 ml deionized water as a control.Add 0.4 ml salicylic acid-sulphuric acid into each tube and mix well, and then incubate all reactions at room temperature for 20 min.Add 9.5 ml of 8% (w/v) NaOH solution into each tube, cool down the tubes (heat is generated due to the reaction) to room temperature (about 20-30 min), and measure the OD410 value with the control for reference.Plot the standard curve with the nitrate concentration as the horizontal axis and the absorbance as the vertical axis. Then, the regression equation can be obtained based on the standard curve (Figure 1). The detailed methods are as follows:Open an Excel, enter the OD410 values in column A and the nitrate concentrations of the standard solutions in column B. Select all the cells containing values, and then insert a scatter plot.Select any data point in the plot, right click, select to add a trend line, choose the linear and display equation, then standard curve and the regression equation are obtained.Figure 1. Standard curve. The 10, 20, 30, 40, 60, 80, 100, and 120 mg/L standard solutions are used to establish a standard curve. Error bars represent SD of biological replicates (n = 4). According to the standard curve, the regression equation is C (µg/ml) = 140.86 x OD410 - 1.1831, where C stands for nitrate concentration.Nitrate assay in Arabidopsis The seedlings are grown on half MS medium for 7 days (as shown in Figure 2), and the seedlings, shoots, and roots are collected separately for nitrate content determination.Figure 2. Hydroponic cultivation system for Arabidopsis seedlings. A. Arabidopsis seeds are grown on a gauze net (250 microns mesh size) that has been sterilized by autoclaving. B. The gauze net is placed on a bracket. Make sure that the medium level in the beaker reaches to the gauze net.Freeze each weighed sample (≤ 0.1 g, for example, about 20-25 7-day-old wildtype seedlings grown on half MS) in a 1.5-ml tube by liquid nitrogen, and grind each sample into powder with the frequency of 30/sec for 1 min using a RETCH MM400.Add 1 ml deionized water into the tubes and boil at 100 °C for 20 min (at least).Centrifuge the samples at 15,871 x g for 10 min, and transfer 0.1 ml supernatant into a new 12-ml tube. Use 0.1 ml deionized water as a control.Add 0.4 ml salicylic acid-sulphuric acid into each tube, mix the sample well, and then incubate the reactions for 20 min at room temperature.Add 9.5 ml of 8% (w/v) NaOH solution into each tube and cool down the tubes to room temperature (about 20-30 min). Measure the OD410 value of each sample with the control for reference.According to the OD410 value obtained in the above step, calculate the nitrate concentration (C) with the regression equation, C (µg/ml) = 140.86 x OD410 - 1.1831 obtained in the Procedure A (Figure 1).Calculate the nitrate content using the following equation: Y = CV/W Where,Y: nitrate content (µg/g), C: nitrate concentration calculated with OD410 into regression equation as step B7 (µg/ml), V: the total volume of extracted sample (ml), W: weight of sample (g). Table 1. The nitrate content of the roots of WT. Seedlings were grown on half MS medium for 7 days and the roots were collected for nitrate determination.Note: The other results of nitrate content in plant tissues were published in the paper of ‘The Arabidopsis NRG2 protein mediates nitrate signaling and interacts with and regulates key nitrate regulators’ (http://www.plantcell.org/content/28/2/485.long). When collecting the seedlings, shoots, and roots, each sample should be harvested within one minute.Each sample should have three replicates at least.When adding salicylic acid-sulphate acid into the tube, the interval time between samples should be the same.When boiling the samples, the boiling time is at least 20 min.The cuvettes used for measuring the OD410 of the samples are quartz cuvettes. 500 mg/L (0.0357 mol/L) KNO3 standard solution0.7221 g KNO3 is dissolved in deionized water, and then add dH2O up to 200 ml Store at 4 °C5% (w/v) salicylic acid-sulphuric acid5 g salicylic acid in 100 ml sulphuric acid Protect from light, store at 4 °C and use within 7 days 8% (w/v) NaOH solution80 g NaOH in 1 L distilled waterStore in a glass bottle with rubber stopper",
  "rotype":"bio-protocol"
}'
```

Respuesta para bio-protocol: 
```
{
  "title":"Nitrate Assay for Plant Tissues", 
  "abstract":"Nitrogen is an essential macronutrient for plant growth and nitrate content in plants can reflect the nitrogen supply of soil. Here, we provide the salicylic acid method to evaluate the nitrate content in plant tissues. The method is reliable and stable, thus it can be a good choice for measurement of nitrate in plant tissues. Nitrogen is an important macronutrient required by plants for normal growth and development. Usually most plants absorb nitrogen mainly in the form of nitrate grown under aerobic conditions (Xu et al., 2016). To determine the nitrate accumulation in plants, we need to test the nitrate content in different tissues of plants. There are some methods for determination of nitrate, for example, potentiometric method (Carlson and Keeney, 1971), phenoldisulfonic acid method (Bremner, 1965), Cadium reduction (Huffman and Barbarick, 1981) and other methods. These methods have some disadvantages, such as lower sensitivity, interferences, technician exposure to carcinogenic chemicals (Cataldo et al., 1975; Vendrell and Zupancic, 1990)Here, we provide the salicylic acid method that is free of interferences, reliable and stable. Nitrosalicylic acid is formed by the reaction of nitrate and salicylic acid under highly acidic conditions. The complex is yellow under basic (pH > 12) condition with maximal absorption at 410 nm. The absorbance is directly proportional to nitrate content. Therefore the nitrate content in tissues can be calculated based on their absorbances. This method is suitable for determination of nitrate concentration in plants.1.5 ml Eppendorf tubes 12-ml plastic culture tube (Greiner Bio One, catalog number: 184261 ) Quartz cuvettes Arabidopsis thaliana roots and/or shoots (7-day-old seedlings) Potassium nitrate (KNO3) (Sinopharm Chemical Reagent, catalog number: 10017218 ) Deionized water MS medium Liquid nitrogen Salicylic acid (Sinopharm Chemical Reagent, catalog number: 30163517 ) Sulphuric acid (98%) (Sinopharm Chemical Reagent, catalog number: 100216008 ) Sodium hydroxide (NaOH) (Sinopharm Chemical Reagent, catalog number: 10019718 ) 500 mg/L (0.0357 mol/L) KNO3 standard solution (see Recipes) 5% (w/v) salicylic acid-sulphuric acid (see Recipes) 8% (w/v) NaOH solution (see Recipes)Excel Standard curveTo make the standard curve, 1 ml, 2 ml, 3 ml, 4 ml, 6 ml, 8 ml, 10 ml, and 12 ml NO3- standard solution (500 mg/L) is transferred to eight 50 ml flasks respectively, and deionized water is added to each solution to bring the total volume to 50 ml. The concentration of the series of standard solution should be 10, 20, 30, 40, 60, 80, 100, and 120 mg/L, respectively. And the molarity of 10, 20, 30, 40, 60, 80, 100, and 120 mg/L KNO3 is 0.0007, 0.0014, 0.0021, 0.0029, 0.0043, 0.0057, 0.0071, 0.0086 mol/L, respectively.Transfer 0.1 ml of each standard solution into a 12-ml tube, respectively. Use 0.1 ml deionized water as a control.Add 0.4 ml salicylic acid-sulphuric acid into each tube and mix well, and then incubate all reactions at room temperature for 20 min.Add 9.5 ml of 8% (w/v) NaOH solution into each tube, cool down the tubes (heat is generated due to the reaction) to room temperature (about 20-30 min), and measure the OD410 value with the control for reference.Plot the standard curve with the nitrate concentration as the horizontal axis and the absorbance as the vertical axis. Then, the regression equation can be obtained based on the standard curve (Figure 1). The detailed methods are as follows:Open an Excel, enter the OD410 values in column A and the nitrate concentrations of the standard solutions in column B. Select all the cells containing values, and then insert a scatter plot.Select any data point in the plot, right click, select to add a trend line, choose the linear and display equation, then standard curve and the regression equation are obtained.Figure 1. Standard curve. The 10, 20, 30, 40, 60, 80, 100, and 120 mg/L standard solutions are used to establish a standard curve. Error bars represent SD of biological replicates (n = 4). According to the standard curve, the regression equation is C (\u00b5g/ml) = 140.86 x OD410 - 1.1831, where C stands for nitrate concentration.Nitrate assay in Arabidopsis The seedlings are grown on half MS medium for 7 days (as shown in Figure 2), and the seedlings, shoots, and roots are collected separately for nitrate content determination.Figure 2. Hydroponic cultivation system for Arabidopsis seedlings. A. Arabidopsis seeds are grown on a gauze net (250 microns mesh size) that has been sterilized by autoclaving. B. The gauze net is placed on a bracket. Make sure that the medium level in the beaker reaches to the gauze net.Freeze each weighed sample (\u2264 0.1 g, for example, about 20-25 7-day-old wildtype seedlings grown on half MS) in a 1.5-ml tube by liquid nitrogen, and grind each sample into powder with the frequency of 30/sec for 1 min using a RETCH MM400.Add 1 ml deionized water into the tubes and boil at 100 \u00b0C for 20 min (at least).Centrifuge the samples at 15,871 x g for 10 min, and transfer 0.1 ml supernatant into a new 12-ml tube. Use 0.1 ml deionized water as a control.Add 0.4 ml salicylic acid-sulphuric acid into each tube, mix the sample well, and then incubate the reactions for 20 min at room temperature.Add 9.5 ml of 8% (w/v) NaOH solution into each tube and cool down the tubes to room temperature (about 20-30 min). Measure the OD410 value of each sample with the control for reference.According to the OD410 value obtained in the above step, calculate the nitrate concentration (C) with the regression equation, C (\u00b5g/ml) = 140.86 x OD410 - 1.1831 obtained in the Procedure A (Figure 1).Calculate the nitrate content using the following equation: Y = CV/W Where,Y: nitrate content (\u00b5g/g), C: nitrate concentration calculated with OD410 into regression equation as step B7 (\u00b5g/ml), V: the total volume of extracted sample (ml), W: weight of sample (g). Table 1. The nitrate content of the roots of WT. Seedlings were grown on half MS medium for 7 days and the roots were collected for nitrate determination.Note: The other results of nitrate content in plant tissues were published in the paper of \u2018The Arabidopsis NRG2 protein mediates nitrate signaling and interacts with and regulates key nitrate regulators\u2019 (http://www.plantcell.org/content/28/2/485.long). When collecting the seedlings, shoots, and roots, each sample should be harvested within one minute.Each sample should have three replicates at least.When adding salicylic acid-sulphate acid into the tube, the interval time between samples should be the same.When boiling the samples, the boiling time is at least 20 min.The cuvettes used for measuring the OD410 of the samples are quartz cuvettes. 500 mg/L (0.0357 mol/L) KNO3 standard solution0.7221 g KNO3 is dissolved in deionized water, and then add dH2O up to 200 ml Store at 4 \u00b0C5% (w/v) salicylic acid-sulphuric acid5 g salicylic acid in 100 ml sulphuric acid Protect from light, store at 4 \u00b0C and use within 7 days 8% (w/v) NaOH solution80 g NaOH in 1 L distilled waterStore in a glass bottle with rubber stopper",
    "type": "bio-protocol",
	"topics": [
		{
			"word": "Biochemistry",
			"porcentaje": "0.51"
		},
		{
			"word": "Plant_Science",
			"porcentaje": "0.97"
		}
	]
}
```


**SourceForge**

Ejemplo CURL:
```
curl -X POST -H 'Content-Type: application/json' -i 'http://herculesapi.elhuyar.eus/thematic' --data '{
    "title":"Streamlink is a CLI utility which pipes video streams",
    "abstract":"Streamlink is a command-line utility that pipes video streams from various services into a video player, such as VLC. The main purpose of Streamlink is to avoid resource-heavy and unoptimized websites, while still allowing the user to enjoy various streamed content. There is also an API available for developers who want access to the stream data. Streamlink is built upon a plugin system that allows support for new services to be easily added. Most of the big streaming services are supported. Streamlink is made up of two parts, a cli and a library API. See their respective sections for more information on how to use them. The default behavior of Streamlink is to playback streams in the VLC player. Please note that by using this application you are bypassing ads run by sites such as Twitch.tv. Please consider donating or paying for subscription services when they are available for the content you consume and enjoy.",
    "rotype":"sourceForge"
}'
```

Respuesta para sourceForge: 
```
{
   "abstract" : "Streamlink is a command-line utility that pipes video streams from various services into a video player, such as VLC. The main purpose of Streamlink is to avoid resource-heavy and unoptimized websites, while still allowing the user to enjoy various streamed content. There is also an API available for developers who want access to the stream data. Streamlink is built upon a plugin system that allows support for new services to be easily added. Most of the big streaming services are supported. Streamlink is made up of two parts, a cli and a library API. See their respective sections for more information on how to use them. The default behavior of Streamlink is to playback streams in the VLC player. Please note that by using this application you are bypassing ads run by sites such as Twitch.tv. Please consider donating or paying for subscription services when they are available for the content you consume and enjoy.",
   "rotype" : "sourceForge",
   "title" : "Streamlink is a CLI utility which pipes video streams",
   "topics" : [
      {
         "porcentaje" : "0.85",
         "word" : "Multimedia"
      },
      {
         "porcentaje" : "0.58",
         "word" : "Communications"
      }
   ]
}
```


## ENDPOINT Términos específicos

La API extrae los términos mas relevantes que aparecen en el texto proporcionado, utilizando el extractor elegido (disponibles: papers=artículos científicos). El clasificador se especifica mediante el parámetro "rotype" en el JSON de entrada. 

## Process requests

URL: https://herculesapi.elhuyar.eus/specific

Método HTTP: POST

```
curl -X POST -H 'Content-Type: application/json' -i 'http://herculesapi.elhuyar.eus/specific' --data '{"rotype":"...", ...}'
```

Datos de entrada (JSON): 
```
{
	"title": "Titulo del paper",
	"abstract": "Abstract del paper",
	"body": "Texto del artículo.",
	"rotype": "tipo de clasificador a utilizar. Este parámetro es obligatorio Valores. posibles: 'papers'=artículos científicos"
}
```

IMPORTANTE:
- Si no se proporciona el parámetro 'body' o es una cadena vacía, el sistema utilizará el clasificador para textos cortos. Si se proporciona este valor, el sistema utilizará el clasificador para textos largos".
- El sistema devuelve un error si los parametros 'title', 'abstract' y 'body' son cadenas vacías.


**Papers**

Ejemplo CURL (sin body, se utiliza el extractor de textos cortos):
```
curl -X POST -H 'Content-Type: application/json' -i 'http://herculesapi.elhuyar.eus/specific' --data '{
    "title":"Revisiting Dockerfiles in Open Source Software Over Time",
    "abstract": "Docker is becoming ubiquitous with containerization for developing and deploying applications. Previous studies have analyzed Dockerfiles that are used to create container images in order to better understand how to improve Docker tooling. These studies obtain Dockerfiles using either Docker Hub or Github. In this paper, we revisit the findings of previous studies using the largest set of Dockerfiles known to date with over 9.4 million unique Dockerfiles found in the World of Code infrastructure spanning from 2013-2020. We contribute a historical view of the Dockerfile format by analyzing the Docker engine changelogs and use the history to enhance our analysis of Dockerfiles. We also reconfirm previous findings of a downward trend in using OS images and an upward trend of using language images. As well, we reconfirm that Dockerfile smell counts are slightly decreasing meaning that Dockerfile authors are likely getting better at following best practices. Based on these findings, it indicates that previous analyses from prior works have been correct in many of their findings and their suggestions to build better tools for Docker image creation are further substantiated.",
    "rotype":"papers"
}'
```

Respuesta para 'papers':
```
{
   "abstract" : "Docker is becoming ubiquitous with containerization for developing and deploying applications. Previous studies have analyzed Dockerfiles that are used to create container images in order to better understand how to improve Docker tooling. These studies obtain Dockerfiles using either Docker Hub or Github. In this paper, we revisit the findings of previous studies using the largest set of Dockerfiles known to date with over 9.4 million unique Dockerfiles found in the World of Code infrastructure spanning from 2013-2020. We contribute a historical view of the Dockerfile format by analyzing the Docker engine changelogs and use the history to enhance our analysis of Dockerfiles. We also reconfirm previous findings of a downward trend in using OS images and an upward trend of using language images. As well, we reconfirm that Dockerfile smell counts are slightly decreasing meaning that Dockerfile authors are likely getting better at following best practices. Based on these findings, it indicates that previous analyses from prior works have been correct in many of their findings and their suggestions to build better tools for Docker image creation are further substantiated.",
   "rotype" : "papers",
   "title" : "Revisiting Dockerfiles in Open Source Software Over Time",
   "topics" : [
      {"porcentaje" : "0.7976", "word" : "dockerfiles"},
      {"porcentaje" : "0.7970", "word" : "previous studies"},
      {"porcentaje" : "0.7542", "word" : "containerization"},
      {"porcentaje" : "0.6976", "word" : "findings"},
      {"porcentaje" : "0.5717", "word" : "open source software"},
      {"porcentaje" : "0.5684", "word" : "suggestions"},
      {"porcentaje" : "0.5030", "word" : "Docker tooling"},
      {"porcentaje" : "0.4986", "word" : "container images"},
      {"porcentaje" : "0.4544", "word" : "dockerfile authors"},
      {"porcentaje" : "0.4417", "word" : "dockerfile smell counts"}
   ]
}
```

