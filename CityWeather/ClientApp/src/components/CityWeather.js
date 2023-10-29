import React, {useEffect, useState} from 'react';
import {Col, Container, Dropdown, DropdownItem, DropdownMenu, DropdownToggle, Row} from 'reactstrap';

export default function CityWeather() {
    const [loading, setLoading] = useState(true);
    const [countryOpen, setCountryOpen] = useState(false);
    const [countryOptions, setCountryOptions] = useState([]);
    const [country, setCountry] = useState('');
    const [cityOpen, setCityOpen] = useState(false);
    const [cityOptions, setCityOptions] = useState([]);
    const [city, setCity] = useState('');
    const [weather, setWeather] = useState(null);

    useEffect(() => {
        getCountries()
            .then(r => {
                setCountryOptions(r);
                setLoading(false);
            })
            .catch(e => console.log(e));
    }, []);

    useEffect(
        () => {
            if (country) {
                getCities(country.code)
                    .then(r => {
                        setCityOptions(r);
                    })
                    .catch(e => console.log(e));
            }
        },
        [country]
    )

    useEffect(
        () => {
            if (city) {
                getCityWeather(city.name)
                    .then(r => {
                        setWeather(r);
                    })
                    .catch(e => console.log(e));
            }
        },
        [city]
    )

    if (loading) {
        return <p>Loading...</p>;
    }

    return (
        <Container>
            <Row>
                <Col xs="3">
                    <h3>Country</h3>
                </Col>
                <Col xs="3">
                    <Dropdown
                        isOpen={countryOpen}
                        toggle={() => {
                            setCountryOpen(!countryOpen);
                        }}>
                        <DropdownToggle caret>
                            {country ? country.name : 'Select a country'}
                        </DropdownToggle>
                        <DropdownMenu>
                            {countryOptions.map((option, index) => (
                                <DropdownItem
                                    key={option.code}
                                    onClick={v => setCountry(option)}>

                                    {option.name}
                                </DropdownItem>
                            ))}
                        </DropdownMenu>
                    </Dropdown>
                </Col>
            </Row>
            <Row>
                <Col xs="3">
                    <h3>City</h3>
                </Col>
                <Col xs="3">
                    <Dropdown
                        isOpen={cityOpen}
                        toggle={() => {
                            setCityOpen(!cityOpen);
                        }}>
                        <DropdownToggle caret>
                            {city ? city.name : 'Select a city'}
                        </DropdownToggle>
                        <DropdownMenu>
                            {cityOptions.map((option, index) => (
                                <DropdownItem
                                    key={option.location}
                                    onClick={v => setCity(option)}>

                                    {option.name}
                                </DropdownItem>
                            ))}
                        </DropdownMenu>
                    </Dropdown>
                </Col>
            </Row>
            <Row>
                <Col xs="3">
                    <h3>Weather</h3>
                </Col>
                <Col xs="auto">
                    {weather ? (
                        <div>
                            <p>Location: {weather.location}</p>
                            <p>Time: {weather.time}</p>
                            {weather.wind ? (
                                <p>Wind: Speed = {weather.wind.speed}, Direction = {weather.wind.deg}</p>
                            ) : (
                                <p>Wind: None</p>
                            )}
                            <p>Visibility: {weather.visibility}</p>
                            <p>Sky Conditions: {weather.skyConditions}</p>
                            <p>Temperature (Celsius): {weather.temperatureCelsius}</p>
                            <p>Temperature (Fahrenheit): {weather.temperatureFahrenheit}</p>
                            <p>Dew Point (Celsius): {weather.dewPointCelsius}</p>
                            <p>Dew Point (Fahrenheit): {weather.dewPointFahrenheit}</p>
                            <p>Relative Humidity: {weather.relativeHumidity}</p>
                            <p>Pressure: {weather.pressure}</p>
                        </div>
                    ) : (
                        <p>Select a city to see the weather</p>
                    )}
                </Col>
            </Row>
        </Container>
    )
}

async function getCountries() {
    const response = await fetch('Countries');
    return await response.json();
}

async function getCities(countryCode) {
    const response = await fetch(`Countries/${countryCode}/Cities`);
    return await response.json();
}

async function getCityWeather(cityName) {
    const response = await fetch(`Weather/${cityName}`);
    return await response.json();
}