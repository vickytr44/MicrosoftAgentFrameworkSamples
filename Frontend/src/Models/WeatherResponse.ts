export enum WeatherCondition {
	Sunny = "Sunny",
	Rainy = "Rainy",
	Cloudy = "Cloudy",
	Snowy = "Snowy",
}

export interface WeatherResponse {
	location: string;
	temperature: string;
	condition: WeatherCondition;
	windSpeed: string;
	humidity: string;
}

export default WeatherResponse;